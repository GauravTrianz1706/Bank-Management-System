@echo off
setlocal enabledelayedexpansion

REM Deploy Docker Image to AWS ECS Fargate
REM This script deploys the containerized application to AWS ECS Fargate

echo ========================================
echo AWS ECS Fargate Deployment Script
echo ========================================
echo.

REM Project configuration
set PROJECT_NAME=bankmanagement-system
set SERVICE_NAME=%PROJECT_NAME%-service
set TASK_FAMILY=%PROJECT_NAME%-task

REM Prompt for AWS configuration
echo AWS Configuration
set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter ECS cluster name (e.g., my-ecs-cluster): "

REM Get AWS Account ID
echo Retrieving AWS Account ID...
for /f "tokens=*" %%i in ('aws sts get-caller-identity --query Account --output text') do set ACCOUNT_ID=%%i
if !ERRORLEVEL! neq 0 (
    echo Failed to retrieve AWS Account ID. Please check your AWS credentials.
    exit /b 1
)
echo AWS Account ID: !ACCOUNT_ID!
echo.

REM Check if cluster exists, create if it doesn't
echo Checking if ECS cluster exists...
aws ecs describe-clusters --clusters "!CLUSTER_NAME!" --region "!AWS_REGION!" >nul 2>&1
if !ERRORLEVEL! neq 0 (
    echo Creating ECS cluster: !CLUSTER_NAME!
    aws ecs create-cluster --cluster-name "!CLUSTER_NAME!" --region "!AWS_REGION!"
)
echo ECS cluster ready: !CLUSTER_NAME!
echo.

REM Prompt for network configuration
echo Network Configuration
set /p VPC_ID="Enter VPC ID (e.g., vpc-0abc123def456): "
set /p SUBNET_IDS="Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): "
set /p SECURITY_GROUP="Enter Security Group ID (e.g., sg-0abc123def): "

REM Parse subnet IDs
for /f "tokens=1,2 delims=," %%a in ("!SUBNET_IDS!") do (
    set SUBNET_1=%%a
    set SUBNET_2=%%b
)
if "!SUBNET_2!"=="" set SUBNET_2=!SUBNET_1!

echo.

REM Prompt for Docker image URI
echo Docker Image Configuration
set /p IMAGE_URI="Enter Docker image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/app:latest): "
echo.

REM Ask about load balancer
echo Load Balancer Configuration
set /p NEED_LB="Do you need a load balancer for this service? (y/n): "

if /i "!NEED_LB!"=="y" (
    echo Creating Application Load Balancer and Target Group...
    
    REM Create target group with target-type ip
    set TG_NAME=%PROJECT_NAME%-tg-%RANDOM%
    for /f "tokens=*" %%i in ('aws elbv2 create-target-group --name "!TG_NAME!" --protocol HTTP --port 8080 --vpc-id "!VPC_ID!" --target-type ip --health-check-enabled --health-check-protocol HTTP --health-check-path /health --health-check-interval-seconds 30 --health-check-timeout-seconds 5 --healthy-threshold-count 2 --unhealthy-threshold-count 3 --region "!AWS_REGION!" --query "TargetGroups[0].TargetGroupArn" --output text') do set TARGET_GROUP_ARN=%%i
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to create target group
        exit /b 1
    )
    
    echo Target Group created: !TARGET_GROUP_ARN!
    
    REM Create Application Load Balancer
    set ALB_NAME=%PROJECT_NAME%-alb
    for /f "tokens=*" %%i in ('aws elbv2 create-load-balancer --name "!ALB_NAME!" --subnets !SUBNET_1! !SUBNET_2! --security-groups "!SECURITY_GROUP!" --scheme internet-facing --type application --ip-address-type ipv4 --region "!AWS_REGION!" --query "LoadBalancers[0].LoadBalancerArn" --output text') do set ALB_ARN=%%i
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to create load balancer
        exit /b 1
    )
    
    echo Load Balancer created: !ALB_ARN!
    
    REM Get ALB DNS name
    for /f "tokens=*" %%i in ('aws elbv2 describe-load-balancers --load-balancer-arns "!ALB_ARN!" --region "!AWS_REGION!" --query "LoadBalancers[0].DNSName" --output text') do set ALB_DNS=%%i
    
    REM Create listener
    aws elbv2 create-listener --load-balancer-arn "!ALB_ARN!" --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn="!TARGET_GROUP_ARN!" --region "!AWS_REGION!" >nul
    
    echo Listener created for load balancer
    echo.
) else (
    echo Skipping load balancer creation
    set TARGET_GROUP_ARN=
    echo.
)

REM Create CloudWatch log group
echo Creating CloudWatch log group...
aws logs create-log-group --log-group-name "/ecs/%PROJECT_NAME%" --region "!AWS_REGION!" 2>nul
echo CloudWatch log group ready: /ecs/%PROJECT_NAME%
echo.

REM Replace placeholders in task definition
echo Preparing task definition...
set TASK_DEF_FILE=ecs\task-definition.json
set TASK_DEF_TEMP=ecs\task-definition-temp.json

powershell -Command "(Get-Content '!TASK_DEF_FILE!') -replace '{{IMAGE_URI}}', '!IMAGE_URI!' -replace '{{AWS_REGION}}', '!AWS_REGION!' -replace '{{ACCOUNT_ID}}', '!ACCOUNT_ID!' | Set-Content '!TASK_DEF_TEMP!'"

REM Register task definition
echo Registering task definition...
for /f "tokens=*" %%i in ('aws ecs register-task-definition --cli-input-json file://!TASK_DEF_TEMP! --region "!AWS_REGION!" --query "taskDefinition.taskDefinitionArn" --output text') do set TASK_DEF_ARN=%%i

if !ERRORLEVEL! neq 0 (
    echo Failed to register task definition
    del /f !TASK_DEF_TEMP! 2>nul
    exit /b 1
)

echo Task definition registered: !TASK_DEF_ARN!
del /f !TASK_DEF_TEMP! 2>nul
echo.

REM Prepare service definition
echo Preparing service definition...
set SERVICE_DEF_FILE=ecs\service-definition.json
set SERVICE_DEF_TEMP=ecs\service-definition-temp.json

if not "!TARGET_GROUP_ARN!"=="" (
    REM Include load balancer configuration
    powershell -Command "(Get-Content '!SERVICE_DEF_FILE!') -replace '{{CLUSTER_NAME}}', '!CLUSTER_NAME!' -replace '{{SUBNET_1}}', '!SUBNET_1!' -replace '{{SUBNET_2}}', '!SUBNET_2!' -replace '{{SECURITY_GROUP}}', '!SECURITY_GROUP!' -replace '{{TARGET_GROUP_ARN}}', '!TARGET_GROUP_ARN!' | Set-Content '!SERVICE_DEF_TEMP!'"
) else (
    REM Remove load balancer section
    powershell -Command "$json = Get-Content '!SERVICE_DEF_FILE!' | ConvertFrom-Json; $json.PSObject.Properties.Remove('loadBalancers'); $json.PSObject.Properties.Remove('healthCheckGracePeriodSeconds'); $json | ConvertTo-Json -Depth 10 | ForEach-Object { $_ -replace '{{CLUSTER_NAME}}', '!CLUSTER_NAME!' -replace '{{SUBNET_1}}', '!SUBNET_1!' -replace '{{SUBNET_2}}', '!SUBNET_2!' -replace '{{SECURITY_GROUP}}', '!SECURITY_GROUP!' } | Set-Content '!SERVICE_DEF_TEMP!'"
)

REM Check if service exists
echo Checking if service exists...
for /f "tokens=*" %%i in ('aws ecs describe-services --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!" --query "services[?status==`ACTIVE`].serviceName" --output text 2^>nul') do set EXISTING_SERVICE=%%i

if not "!EXISTING_SERVICE!"=="" if not "!EXISTING_SERVICE!"=="None" (
    echo Service exists. Updating service...
    aws ecs update-service --cluster "!CLUSTER_NAME!" --service "!SERVICE_NAME!" --task-definition "!TASK_DEF_ARN!" --region "!AWS_REGION!" >nul
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to update service
        del /f !SERVICE_DEF_TEMP! 2>nul
        exit /b 1
    )
    
    echo Service updated successfully
) else (
    echo Creating new service...
    aws ecs create-service --cli-input-json file://!SERVICE_DEF_TEMP! --region "!AWS_REGION!" >nul
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to create service
        del /f !SERVICE_DEF_TEMP! 2>nul
        exit /b 1
    )
    
    echo Service created successfully
)

del /f !SERVICE_DEF_TEMP! 2>nul
echo.

REM Wait for service to stabilize
echo Waiting for service to stabilize (this may take a few minutes)...
aws ecs wait services-stable --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!"

if !ERRORLEVEL! neq 0 (
    echo Service failed to stabilize. Check ECS console for details.
    exit /b 1
)

echo Service is stable!
echo.

REM Display deployment information
echo ========================================
echo Deployment Successful!
echo ========================================
echo.
echo Deployment Details:
echo   Cluster: !CLUSTER_NAME!
echo   Service: !SERVICE_NAME!
echo   Task Definition: !TASK_DEF_ARN!
echo   Region: !AWS_REGION!
echo.

if not "!ALB_DNS!"=="" (
    echo Application Access:
    echo   Load Balancer DNS: http://!ALB_DNS!
    echo   Health Check: http://!ALB_DNS!/health
    echo.
)

echo CloudWatch Logs:
echo   Log Group: /ecs/%PROJECT_NAME%
echo   View logs: aws logs tail /ecs/%PROJECT_NAME% --follow --region !AWS_REGION!
echo.

echo Service Status:
aws ecs describe-services --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!" --query "services[0].[serviceName,status,runningCount,desiredCount]" --output table

echo.
echo Deployment complete!

endlocal
