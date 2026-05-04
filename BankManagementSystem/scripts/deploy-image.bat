@echo off
setlocal enabledelayedexpansion

REM Deploy BankManagementSystem to AWS ECS Fargate (Windows)
REM This script deploys the containerized application to AWS ECS

echo ==========================================
echo AWS ECS Fargate Deployment Script
echo ==========================================
echo.

REM Project configuration
set PROJECT_NAME=bankmanagementsystem
set TASK_FAMILY=!PROJECT_NAME!-task
set SERVICE_NAME=!PROJECT_NAME!-service

REM Prompt for AWS configuration
echo === AWS Configuration ===
set /p AWS_REGION="Enter AWS Region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter ECS Cluster Name (e.g., my-ecs-cluster): "
echo.

REM Prompt for network configuration
echo === Network Configuration ===
set /p VPC_ID="Enter VPC ID (e.g., vpc-0abc123def456): "
set /p SUBNETS_INPUT="Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): "
set /p SECURITY_GROUP="Enter Security Group ID (e.g., sg-0abc123def): "
echo.

REM Parse subnets
for /f "tokens=1,2 delims=," %%a in ("!SUBNETS_INPUT!") do (
    set SUBNET_1=%%a
    set SUBNET_2=%%b
)
if "!SUBNET_2!"=="" set SUBNET_2=!SUBNET_1!

REM Prompt for Docker image
echo === Docker Image Configuration ===
set /p IMAGE_URI="Enter Docker Image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/bankmanagementsystem:latest): "
echo.

REM Get AWS Account ID
echo Retrieving AWS Account ID...
for /f "delims=" %%i in ('aws sts get-caller-identity --query Account --output text') do set ACCOUNT_ID=%%i
echo AWS Account ID: !ACCOUNT_ID!
echo.

REM Check if ECS cluster exists
echo Checking ECS cluster...
aws ecs describe-clusters --clusters !CLUSTER_NAME! --region !AWS_REGION! >nul 2>&1
if !ERRORLEVEL! neq 0 (
    echo Creating ECS cluster: !CLUSTER_NAME!
    aws ecs create-cluster --cluster-name !CLUSTER_NAME! --region !AWS_REGION!
)
echo.

REM Prompt for load balancer
echo === Load Balancer Configuration ===
set /p NEED_LB="Do you need a load balancer for this service? (y/n): "
echo.

set TARGET_GROUP_ARN=
if /i "!NEED_LB!"=="y" (
    echo Creating Application Load Balancer and Target Group...
    
    set ALB_NAME=!PROJECT_NAME!-alb
    set TG_NAME=!PROJECT_NAME!-tg
    
    REM Create Target Group with target-type ip
    for /f "delims=" %%i in ('aws elbv2 create-target-group --name !TG_NAME! --protocol HTTP --port 8080 --vpc-id !VPC_ID! --target-type ip --health-check-enabled --health-check-protocol HTTP --health-check-path "/health/" --health-check-interval-seconds 30 --health-check-timeout-seconds 5 --healthy-threshold-count 2 --unhealthy-threshold-count 3 --region !AWS_REGION! --query "TargetGroups[0].TargetGroupArn" --output text 2^>nul') do set TARGET_GROUP_ARN=%%i
    
    if "!TARGET_GROUP_ARN!"=="" (
        echo WARNING: Could not create Target Group, attempting to find existing...
        for /f "delims=" %%i in ('aws elbv2 describe-target-groups --names !TG_NAME! --region !AWS_REGION! --query "TargetGroups[0].TargetGroupArn" --output text 2^>nul') do set TARGET_GROUP_ARN=%%i
    )
    
    echo Target Group ARN: !TARGET_GROUP_ARN!
    echo.
)

REM Create CloudWatch log group
echo Creating CloudWatch log group...
aws logs create-log-group --log-group-name "/ecs/!PROJECT_NAME!" --region !AWS_REGION! 2>nul
echo.

REM Prepare task definition
echo Preparing task definition...
set TASK_DEF_FILE=ecs\task-definition.json
set TASK_DEF_TEMP=ecs\task-definition-temp.json

copy /y !TASK_DEF_FILE! !TASK_DEF_TEMP! >nul

REM Replace placeholders using PowerShell
powershell -Command "(Get-Content !TASK_DEF_TEMP!) -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Set-Content !TASK_DEF_TEMP!"
powershell -Command "(Get-Content !TASK_DEF_TEMP!) -replace '{{AWS_REGION}}', '!AWS_REGION!' | Set-Content !TASK_DEF_TEMP!"
powershell -Command "(Get-Content !TASK_DEF_TEMP!) -replace '{{ACCOUNT_ID}}', '!ACCOUNT_ID!' | Set-Content !TASK_DEF_TEMP!"

echo Registering task definition...
for /f "delims=" %%i in ('aws ecs register-task-definition --cli-input-json file://!TASK_DEF_TEMP! --region !AWS_REGION! --query "taskDefinition.taskDefinitionArn" --output text') do set TASK_DEF_ARN=%%i

echo Task Definition ARN: !TASK_DEF_ARN!
echo.

REM Prepare service definition
echo Preparing service definition...
set SERVICE_DEF_FILE=ecs\service-definition.json
set SERVICE_DEF_TEMP=ecs\service-definition-temp.json

copy /y !SERVICE_DEF_FILE! !SERVICE_DEF_TEMP! >nul

REM Replace placeholders
powershell -Command "(Get-Content !SERVICE_DEF_TEMP!) -replace '{{CLUSTER_NAME}}', '!CLUSTER_NAME!' | Set-Content !SERVICE_DEF_TEMP!"
powershell -Command "(Get-Content !SERVICE_DEF_TEMP!) -replace '{{SUBNET_1}}', '!SUBNET_1!' | Set-Content !SERVICE_DEF_TEMP!"
powershell -Command "(Get-Content !SERVICE_DEF_TEMP!) -replace '{{SUBNET_2}}', '!SUBNET_2!' | Set-Content !SERVICE_DEF_TEMP!"
powershell -Command "(Get-Content !SERVICE_DEF_TEMP!) -replace '{{SECURITY_GROUP}}', '!SECURITY_GROUP!' | Set-Content !SERVICE_DEF_TEMP!"

REM Handle load balancer configuration
if "!TARGET_GROUP_ARN!"=="" (
    powershell -Command "$json = Get-Content !SERVICE_DEF_TEMP! | ConvertFrom-Json; $json.PSObject.Properties.Remove('loadBalancers'); $json.PSObject.Properties.Remove('healthCheckGracePeriodSeconds'); $json | ConvertTo-Json -Depth 10 | Set-Content !SERVICE_DEF_TEMP!"
) else (
    powershell -Command "(Get-Content !SERVICE_DEF_TEMP!) -replace '{{TARGET_GROUP_ARN}}', '!TARGET_GROUP_ARN!' | Set-Content !SERVICE_DEF_TEMP!"
)

REM Check if service exists
echo Checking if service exists...
for /f "delims=" %%i in ('aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION! --query "services[?status==`ACTIVE`].serviceName" --output text 2^>nul') do set EXISTING_SERVICE=%%i

if "!EXISTING_SERVICE!"=="" (
    echo Creating new ECS service...
    aws ecs create-service --cli-input-json file://!SERVICE_DEF_TEMP! --region !AWS_REGION!
) else (
    echo Updating existing ECS service...
    aws ecs update-service --cluster !CLUSTER_NAME! --service !SERVICE_NAME! --task-definition !TASK_DEF_ARN! --desired-count 2 --region !AWS_REGION!
)

echo.
echo Waiting for service to become stable...
aws ecs wait services-stable --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION!

echo.
echo ==========================================
echo Deployment Completed Successfully!
echo ==========================================
echo.

REM Display service information
echo Service Details:
aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION! --query "services[0].[serviceName,status,runningCount,desiredCount]" --output table

echo.
echo CloudWatch Logs: /ecs/!PROJECT_NAME!
echo Region: !AWS_REGION!
echo.
echo To view logs:
echo   aws logs tail /ecs/!PROJECT_NAME! --follow --region !AWS_REGION!
echo.

REM Cleanup temp files
del /f /q !TASK_DEF_TEMP! !SERVICE_DEF_TEMP! 2>nul

endlocal
