@echo off
setlocal enabledelayedexpansion

REM Deploy BankManagementSystem to AWS ECS Fargate
REM This script deploys the containerized application to AWS ECS

echo ========================================
echo BankManagementSystem - ECS Deployment
echo ========================================
echo.

REM Project configuration
set PROJECT_NAME=bankmanagement-system
set TASK_FAMILY=bankmanagement-system-task
set SERVICE_NAME=bankmanagement-system-service
set LOG_GROUP=/ecs/bankmanagement-system

REM Prompt for AWS configuration
echo === AWS Configuration ===
set /p AWS_REGION="Enter AWS Region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter ECS Cluster Name (e.g., my-ecs-cluster): "

REM Get AWS Account ID
echo Retrieving AWS Account ID...
for /f "delims=" %%i in ('aws sts get-caller-identity --query Account --output text') do set ACCOUNT_ID=%%i
echo Account ID: !ACCOUNT_ID!

REM Check if cluster exists, create if it doesn't
echo Checking ECS cluster...
aws ecs describe-clusters --clusters "!CLUSTER_NAME!" --region "!AWS_REGION!" >nul 2>&1
if !ERRORLEVEL! neq 0 (
    echo Cluster does not exist. Creating ECS cluster: !CLUSTER_NAME!
    aws ecs create-cluster --cluster-name "!CLUSTER_NAME!" --region "!AWS_REGION!"
    if !ERRORLEVEL! neq 0 (
        echo Failed to create ECS cluster
        exit /b 1
    )
    echo ECS cluster created successfully
)

REM Prompt for network configuration
echo.
echo === Network Configuration ===
set /p VPC_ID="Enter VPC ID (e.g., vpc-0abc123def456): "
set /p SUBNETS_INPUT="Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): "
set /p SECURITY_GROUP="Enter Security Group ID (e.g., sg-0abc123def): "

REM Parse subnets
for /f "tokens=1,2 delims=," %%a in ("!SUBNETS_INPUT!") do (
    set SUBNET_1=%%a
    set SUBNET_2=%%b
)
if "!SUBNET_2!"=="" set SUBNET_2=!SUBNET_1!

REM Prompt for Docker image
echo.
echo === Docker Image Configuration ===
set /p IMAGE_URI="Enter Docker Image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/bankmanagement-system:latest): "

REM Prompt for database configuration
echo.
echo === Database Configuration ===
set /p DB_HOST="Enter Database Host: "
set /p DB_NAME="Enter Database Name (default: OpenBankLocal): "
if "!DB_NAME!"=="" set DB_NAME=OpenBankLocal
set /p DB_USER="Enter Database User: "

REM Prompt for load balancer
echo.
echo === Load Balancer Configuration ===
set /p NEED_LB="Do you need a load balancer for this service? (y/n): "

if /i "!NEED_LB!"=="y" (
    echo Creating Application Load Balancer...
    
    REM Create ALB
    set ALB_NAME=!PROJECT_NAME!-alb
    for /f "delims=" %%i in ('aws elbv2 create-load-balancer --name "!ALB_NAME!" --subnets "!SUBNET_1!" "!SUBNET_2!" --security-groups "!SECURITY_GROUP!" --scheme internet-facing --type application --ip-address-type ipv4 --region "!AWS_REGION!" --query "LoadBalancers[0].LoadBalancerArn" --output text 2^>nul') do set ALB_ARN=%%i
    
    if "!ALB_ARN!"=="" (
        echo Load balancer may already exist, attempting to retrieve...
        for /f "delims=" %%i in ('aws elbv2 describe-load-balancers --names "!ALB_NAME!" --region "!AWS_REGION!" --query "LoadBalancers[0].LoadBalancerArn" --output text 2^>nul') do set ALB_ARN=%%i
    )
    
    if "!ALB_ARN!"=="" (
        echo Failed to create or retrieve load balancer
        exit /b 1
    )
    
    echo Load Balancer ARN: !ALB_ARN!
    
    REM Create Target Group with ip target type
    set TG_NAME=!PROJECT_NAME!-tg
    for /f "delims=" %%i in ('aws elbv2 create-target-group --name "!TG_NAME!" --protocol HTTP --port 8080 --vpc-id "!VPC_ID!" --target-type ip --health-check-enabled --health-check-protocol HTTP --health-check-path /health --health-check-interval-seconds 30 --health-check-timeout-seconds 5 --healthy-threshold-count 2 --unhealthy-threshold-count 3 --region "!AWS_REGION!" --query "TargetGroups[0].TargetGroupArn" --output text 2^>nul') do set TARGET_GROUP_ARN=%%i
    
    if "!TARGET_GROUP_ARN!"=="" (
        echo Target group may already exist, attempting to retrieve...
        for /f "delims=" %%i in ('aws elbv2 describe-target-groups --names "!TG_NAME!" --region "!AWS_REGION!" --query "TargetGroups[0].TargetGroupArn" --output text 2^>nul') do set TARGET_GROUP_ARN=%%i
    )
    
    if "!TARGET_GROUP_ARN!"=="" (
        echo Failed to create or retrieve target group
        exit /b 1
    )
    
    echo Target Group ARN: !TARGET_GROUP_ARN!
    
    REM Create Listener
    for /f "delims=" %%i in ('aws elbv2 create-listener --load-balancer-arn "!ALB_ARN!" --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn="!TARGET_GROUP_ARN!" --region "!AWS_REGION!" --query "Listeners[0].ListenerArn" --output text 2^>nul') do set LISTENER_ARN=%%i
    
    if "!LISTENER_ARN!"=="" (
        echo Listener may already exist
    ) else (
        echo Listener created successfully
    )
    
    REM Get ALB DNS name
    for /f "delims=" %%i in ('aws elbv2 describe-load-balancers --load-balancer-arns "!ALB_ARN!" --region "!AWS_REGION!" --query "LoadBalancers[0].DNSName" --output text') do set ALB_DNS=%%i
    
    echo Load Balancer DNS: !ALB_DNS!
) else (
    set TARGET_GROUP_ARN=
    echo Skipping load balancer configuration
)

REM Create CloudWatch Log Group
echo.
echo Creating CloudWatch Log Group...
aws logs create-log-group --log-group-name "!LOG_GROUP!" --region "!AWS_REGION!" 2>nul
if !ERRORLEVEL! neq 0 (
    echo Log group already exists
)

REM Update task definition JSON
echo.
echo Preparing task definition...
set TASK_DEF_FILE=ecs\task-definition.json

REM Create temporary file with replacements
copy "!TASK_DEF_FILE!" "!TASK_DEF_FILE!.tmp" >nul

powershell -Command "(Get-Content '!TASK_DEF_FILE!.tmp') -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Set-Content '!TASK_DEF_FILE!.tmp'"
powershell -Command "(Get-Content '!TASK_DEF_FILE!.tmp') -replace '{{AWS_REGION}}', '!AWS_REGION!' | Set-Content '!TASK_DEF_FILE!.tmp'"
powershell -Command "(Get-Content '!TASK_DEF_FILE!.tmp') -replace '{{ACCOUNT_ID}}', '!ACCOUNT_ID!' | Set-Content '!TASK_DEF_FILE!.tmp'"
powershell -Command "(Get-Content '!TASK_DEF_FILE!.tmp') -replace '{{DB_HOST}}', '!DB_HOST!' | Set-Content '!TASK_DEF_FILE!.tmp'"
powershell -Command "(Get-Content '!TASK_DEF_FILE!.tmp') -replace '{{DB_NAME}}', '!DB_NAME!' | Set-Content '!TASK_DEF_FILE!.tmp'"
powershell -Command "(Get-Content '!TASK_DEF_FILE!.tmp') -replace '{{DB_USER}}', '!DB_USER!' | Set-Content '!TASK_DEF_FILE!.tmp'"

REM Register task definition
echo Registering task definition...
for /f "delims=" %%i in ('aws ecs register-task-definition --cli-input-json file://!TASK_DEF_FILE!.tmp --region "!AWS_REGION!" --query "taskDefinition.taskDefinitionArn" --output text') do set TASK_DEF_ARN=%%i

if "!TASK_DEF_ARN!"=="" (
    echo Failed to register task definition
    del "!TASK_DEF_FILE!.tmp"
    exit /b 1
)

echo Task Definition registered: !TASK_DEF_ARN!

REM Clean up temporary file
del "!TASK_DEF_FILE!.tmp"

REM Update service definition JSON
echo.
echo Preparing service definition...
set SERVICE_DEF_FILE=ecs\service-definition.json

REM Create temporary file with replacements
copy "!SERVICE_DEF_FILE!" "!SERVICE_DEF_FILE!.tmp" >nul

powershell -Command "(Get-Content '!SERVICE_DEF_FILE!.tmp') -replace '{{CLUSTER_NAME}}', '!CLUSTER_NAME!' | Set-Content '!SERVICE_DEF_FILE!.tmp'"
powershell -Command "(Get-Content '!SERVICE_DEF_FILE!.tmp') -replace '{{SUBNET_1}}', '!SUBNET_1!' | Set-Content '!SERVICE_DEF_FILE!.tmp'"
powershell -Command "(Get-Content '!SERVICE_DEF_FILE!.tmp') -replace '{{SUBNET_2}}', '!SUBNET_2!' | Set-Content '!SERVICE_DEF_FILE!.tmp'"
powershell -Command "(Get-Content '!SERVICE_DEF_FILE!.tmp') -replace '{{SECURITY_GROUP}}', '!SECURITY_GROUP!' | Set-Content '!SERVICE_DEF_FILE!.tmp'"

if not "!TARGET_GROUP_ARN!"=="" (
    powershell -Command "(Get-Content '!SERVICE_DEF_FILE!.tmp') -replace '{{TARGET_GROUP_ARN}}', '!TARGET_GROUP_ARN!' | Set-Content '!SERVICE_DEF_FILE!.tmp'"
) else (
    REM Remove loadBalancers section if no load balancer
    powershell -Command "$content = Get-Content '!SERVICE_DEF_FILE!.tmp' -Raw; $content = $content -replace '(?s)\"loadBalancers\":\s*\[.*?\],\s*', ''; $content = $content -replace '\"healthCheckGracePeriodSeconds\":\s*\d+,\s*', ''; $content | Set-Content '!SERVICE_DEF_FILE!.tmp'"
)

REM Check if service exists
echo Checking if service exists...
for /f "delims=" %%i in ('aws ecs describe-services --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!" --query "services[?status==`ACTIVE`].serviceName" --output text 2^>nul') do set EXISTING_SERVICE=%%i

if "!EXISTING_SERVICE!"=="" (
    REM Create new service
    echo Creating new ECS service...
    aws ecs create-service --cli-input-json file://!SERVICE_DEF_FILE!.tmp --region "!AWS_REGION!"
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to create service
        del "!SERVICE_DEF_FILE!.tmp"
        exit /b 1
    )
    
    echo Service created successfully
) else (
    REM Update existing service
    echo Updating existing ECS service...
    aws ecs update-service --cluster "!CLUSTER_NAME!" --service "!SERVICE_NAME!" --task-definition "!TASK_DEF_ARN!" --force-new-deployment --region "!AWS_REGION!"
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to update service
        del "!SERVICE_DEF_FILE!.tmp"
        exit /b 1
    )
    
    echo Service updated successfully
)

REM Clean up temporary file
del "!SERVICE_DEF_FILE!.tmp"

REM Wait for service to stabilize
echo.
echo Waiting for service to stabilize (this may take a few minutes)...
aws ecs wait services-stable --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!"

if !ERRORLEVEL! equ 0 (
    echo Service is stable
) else (
    echo Service stabilization timed out, but deployment may still succeed
)

REM Verify deployment
echo.
echo Verifying deployment...
for /f "delims=" %%i in ('aws ecs describe-services --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!" --query "services[0].runningCount" --output text') do set RUNNING_COUNT=%%i

echo.
echo ========================================
echo Deployment Completed Successfully!
echo ========================================
echo.
echo Deployment Details:
echo   Cluster: !CLUSTER_NAME!
echo   Service: !SERVICE_NAME!
echo   Task Definition: !TASK_DEF_ARN!
echo   Running Tasks: !RUNNING_COUNT!
echo   CloudWatch Logs: !LOG_GROUP!

if not "!ALB_DNS!"=="" (
    echo   Load Balancer: http://!ALB_DNS!
    echo   Health Check: http://!ALB_DNS!/health
)

echo.
echo Useful Commands:
echo   View logs: aws logs tail !LOG_GROUP! --follow --region !AWS_REGION!
echo   Service status: aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION!
echo   List tasks: aws ecs list-tasks --cluster !CLUSTER_NAME! --service-name !SERVICE_NAME! --region !AWS_REGION!
echo.

endlocal
