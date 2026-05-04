@echo off
setlocal enabledelayedexpansion

REM Deploy to AWS ECS Fargate Script for BankDatabaseAccess
REM This script deploys the Docker image to AWS ECS Fargate

echo ========================================
echo AWS ECS Fargate Deployment Script
echo ========================================
echo.

REM Project configuration
set PROJECT_NAME=bankdatabaseaccess
set TASK_FAMILY=bankdatabaseaccess-task
set SERVICE_NAME=bankdatabaseaccess-service

REM Prompt for AWS configuration
echo === AWS Configuration ===
set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter ECS cluster name (e.g., my-ecs-cluster): "

REM Get AWS Account ID
echo Retrieving AWS Account ID...
for /f "tokens=*" %%i in ('aws sts get-caller-identity --query Account --output text') do set ACCOUNT_ID=%%i
echo Account ID: !ACCOUNT_ID!

REM Check if cluster exists, create if not
echo Checking ECS cluster...
aws ecs describe-clusters --clusters "!CLUSTER_NAME!" --region "!AWS_REGION!" >nul 2>&1
if !ERRORLEVEL! neq 0 (
    echo Creating ECS cluster: !CLUSTER_NAME!
    aws ecs create-cluster --cluster-name "!CLUSTER_NAME!" --region "!AWS_REGION!"
)
echo Cluster ready: !CLUSTER_NAME!

REM Prompt for network configuration
echo.
echo === Network Configuration ===
set /p VPC_ID="Enter VPC ID (e.g., vpc-0abc123def456): "
set /p SUBNET_IDS="Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): "
set /p SECURITY_GROUP="Enter Security Group ID (e.g., sg-0abc123def): "

REM Parse subnets
for /f "tokens=1,2 delims=," %%a in ("!SUBNET_IDS!") do (
    set SUBNET_1=%%a
    set SUBNET_2=%%b
)
if "!SUBNET_2!"=="" set SUBNET_2=!SUBNET_1!

REM Prompt for Docker image
echo.
echo === Docker Image Configuration ===
set /p IMAGE_URI="Enter Docker image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/app:latest): "

REM Prompt for database configuration
echo.
echo === Database Configuration ===
set /p DB_SERVER="Enter database server (e.g., mydb.region.rds.amazonaws.com): "
set /p DB_NAME="Enter database name: "
set /p DB_USER="Enter database user: "
set /p DB_PASSWORD="Enter database password: "

REM Build connection string
set DB_CONNECTION_STRING=Server=!DB_SERVER!;Database=!DB_NAME!;User Id=!DB_USER!;Password=!DB_PASSWORD!;TrustServerCertificate=True;

REM Prompt for load balancer
echo.
set /p NEED_LB="Do you need a load balancer for this service? (y/n): "

set TARGET_GROUP_ARN=
set ALB_DNS=

if /i "!NEED_LB!"=="y" (
    echo Creating Application Load Balancer...
    
    REM Create ALB
    set ALB_NAME=!PROJECT_NAME!-alb
    for /f "tokens=*" %%i in ('aws elbv2 create-load-balancer --name "!ALB_NAME!" --subnets "!SUBNET_1!" "!SUBNET_2!" --security-groups "!SECURITY_GROUP!" --scheme internet-facing --type application --ip-address-type ipv4 --region "!AWS_REGION!" --query "LoadBalancers[0].LoadBalancerArn" --output text 2^>nul') do set ALB_ARN=%%i
    
    if "!ALB_ARN!"=="" (
        REM ALB might already exist
        for /f "tokens=*" %%i in ('aws elbv2 describe-load-balancers --names "!ALB_NAME!" --region "!AWS_REGION!" --query "LoadBalancers[0].LoadBalancerArn" --output text 2^>nul') do set ALB_ARN=%%i
    )
    
    if "!ALB_ARN!"=="" (
        echo Failed to create or find load balancer
        exit /b 1
    )
    
    echo Load Balancer ARN: !ALB_ARN!
    
    REM Create Target Group with ip target type
    set TG_NAME=!PROJECT_NAME!-tg
    for /f "tokens=*" %%i in ('aws elbv2 create-target-group --name "!TG_NAME!" --protocol HTTP --port 8080 --vpc-id "!VPC_ID!" --target-type ip --health-check-enabled --health-check-protocol HTTP --health-check-path /health --health-check-interval-seconds 30 --health-check-timeout-seconds 5 --healthy-threshold-count 2 --unhealthy-threshold-count 3 --region "!AWS_REGION!" --query "TargetGroups[0].TargetGroupArn" --output text 2^>nul') do set TARGET_GROUP_ARN=%%i
    
    if "!TARGET_GROUP_ARN!"=="" (
        REM Target group might already exist
        for /f "tokens=*" %%i in ('aws elbv2 describe-target-groups --names "!TG_NAME!" --region "!AWS_REGION!" --query "TargetGroups[0].TargetGroupArn" --output text 2^>nul') do set TARGET_GROUP_ARN=%%i
    )
    
    if "!TARGET_GROUP_ARN!"=="" (
        echo Failed to create or find target group
        exit /b 1
    )
    
    echo Target Group ARN: !TARGET_GROUP_ARN!
    
    REM Create listener
    aws elbv2 create-listener --load-balancer-arn "!ALB_ARN!" --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn="!TARGET_GROUP_ARN!" --region "!AWS_REGION!" >nul 2>&1
    
    REM Get ALB DNS name
    for /f "tokens=*" %%i in ('aws elbv2 describe-load-balancers --load-balancer-arns "!ALB_ARN!" --region "!AWS_REGION!" --query "LoadBalancers[0].DNSName" --output text') do set ALB_DNS=%%i
    
    echo Load Balancer DNS: !ALB_DNS!
) else (
    echo Skipping load balancer configuration
)

REM Create CloudWatch log group
echo.
echo Creating CloudWatch log group...
aws logs create-log-group --log-group-name "/ecs/!PROJECT_NAME!" --region "!AWS_REGION!" 2>nul

REM Update task definition JSON
echo.
echo Preparing task definition...
cd /d "%~dp0\.."

REM Create temporary task definition with replacements
powershell -Command "(Get-Content ecs\task-definition.json) -replace '{{IMAGE_URI}}', '!IMAGE_URI!' -replace '{{AWS_REGION}}', '!AWS_REGION!' -replace '{{ACCOUNT_ID}}', '!ACCOUNT_ID!' -replace '{{DB_CONNECTION_STRING}}', '!DB_CONNECTION_STRING!' -replace '{{DB_SERVER}}', '!DB_SERVER!' -replace '{{DB_NAME}}', '!DB_NAME!' -replace '{{DB_USER}}', '!DB_USER!' -replace '{{DB_PASSWORD}}', '!DB_PASSWORD!' | Set-Content -Path '%TEMP%\task-definition-temp.json'"

REM Register task definition
echo Registering task definition...
for /f "tokens=*" %%i in ('aws ecs register-task-definition --cli-input-json file:///%TEMP%\task-definition-temp.json --region "!AWS_REGION!" --query "taskDefinition.taskDefinitionArn" --output text') do set TASK_DEF_ARN=%%i

if "!TASK_DEF_ARN!"=="" (
    echo Failed to register task definition
    del "%TEMP%\task-definition-temp.json" 2>nul
    exit /b 1
)

echo Task definition registered: !TASK_DEF_ARN!

REM Clean up temp file
del "%TEMP%\task-definition-temp.json" 2>nul

REM Update service definition JSON
if not "!TARGET_GROUP_ARN!"=="" (
    REM With load balancer
    powershell -Command "(Get-Content ecs\service-definition.json) -replace '{{CLUSTER_NAME}}', '!CLUSTER_NAME!' -replace '{{SUBNET_1}}', '!SUBNET_1!' -replace '{{SUBNET_2}}', '!SUBNET_2!' -replace '{{SECURITY_GROUP}}', '!SECURITY_GROUP!' -replace '{{TARGET_GROUP_ARN}}', '!TARGET_GROUP_ARN!' | Set-Content -Path '%TEMP%\service-definition-temp.json'"
) else (
    REM Without load balancer - remove loadBalancers section
    powershell -Command "$json = Get-Content ecs\service-definition.json | ConvertFrom-Json; $json.PSObject.Properties.Remove('loadBalancers'); $json.PSObject.Properties.Remove('healthCheckGracePeriodSeconds'); $json = $json | ConvertTo-Json -Depth 10; $json -replace '{{CLUSTER_NAME}}', '!CLUSTER_NAME!' -replace '{{SUBNET_1}}', '!SUBNET_1!' -replace '{{SUBNET_2}}', '!SUBNET_2!' -replace '{{SECURITY_GROUP}}', '!SECURITY_GROUP!' | Set-Content -Path '%TEMP%\service-definition-temp.json'"
)

REM Check if service exists
echo.
echo Checking if service exists...
for /f "tokens=*" %%i in ('aws ecs describe-services --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!" --query "services[?serviceName==`!SERVICE_NAME!`].serviceName" --output text 2^>nul') do set SERVICE_EXISTS=%%i

if "!SERVICE_EXISTS!"=="" (
    REM Create new service
    echo Creating new ECS service...
    aws ecs create-service --cli-input-json file:///%TEMP%\service-definition-temp.json --region "!AWS_REGION!"
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to create service
        del "%TEMP%\service-definition-temp.json" 2>nul
        exit /b 1
    )
    
    echo Service created successfully!
) else (
    REM Update existing service
    echo Updating existing ECS service...
    aws ecs update-service --cluster "!CLUSTER_NAME!" --service "!SERVICE_NAME!" --task-definition "!TASK_DEF_ARN!" --desired-count 2 --region "!AWS_REGION!"
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to update service
        del "%TEMP%\service-definition-temp.json" 2>nul
        exit /b 1
    )
    
    echo Service updated successfully!
)

REM Clean up temp file
del "%TEMP%\service-definition-temp.json" 2>nul

REM Wait for service to stabilize
echo.
echo Waiting for service to stabilize (this may take a few minutes)...
aws ecs wait services-stable --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!"

REM Verify deployment
echo.
echo === Deployment Status ===
aws ecs describe-services --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!" --query "services[0].[serviceName,status,runningCount,desiredCount]" --output table

echo.
echo ========================================
echo Deployment Completed Successfully!
echo ========================================
echo.
echo Service Details:
echo   Cluster: !CLUSTER_NAME!
echo   Service: !SERVICE_NAME!
echo   Region: !AWS_REGION!
echo.

if not "!ALB_DNS!"=="" (
    echo Application Access:
    echo   Load Balancer: http://!ALB_DNS!
    echo   Health Check: http://!ALB_DNS!/health
    echo.
)

echo CloudWatch Logs:
echo   Log Group: /ecs/!PROJECT_NAME!
echo   Region: !AWS_REGION!
echo.
echo Useful Commands:
echo   View logs: aws logs tail /ecs/!PROJECT_NAME! --follow --region !AWS_REGION!
echo   List tasks: aws ecs list-tasks --cluster !CLUSTER_NAME! --service-name !SERVICE_NAME! --region !AWS_REGION!
echo   Describe service: aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION!
echo.

endlocal
