# AWS Deployment Guide - Bank Management System

## Prerequisites

- AWS Account with appropriate permissions
- AWS CLI configured
- Docker installed (for containerization - separate workflow)
- .NET Framework 4.7.2 or higher

## Infrastructure Setup

### 1. VPC and Networking

```bash
# Create VPC
aws ec2 create-vpc --cidr-block 10.0.0.0/16 --tag-specifications 'ResourceType=vpc,Tags=[{Key=Name,Value=BankApp-VPC}]'

# Create subnets (at least 2 for high availability)
aws ec2 create-subnet --vpc-id <vpc-id> --cidr-block 10.0.1.0/24 --availability-zone us-east-1a
aws ec2 create-subnet --vpc-id <vpc-id> --cidr-block 10.0.2.0/24 --availability-zone us-east-1b

# Create Internet Gateway
aws ec2 create-internet-gateway --tag-specifications 'ResourceType=internet-gateway,Tags=[{Key=Name,Value=BankApp-IGW}]'
aws ec2 attach-internet-gateway --vpc-id <vpc-id> --internet-gateway-id <igw-id>
```

### 2. Amazon RDS for SQL Server

```bash
# Create RDS instance
aws rds create-db-instance \
  --db-instance-identifier bankapp-db \
  --db-instance-class db.t3.medium \
  --engine sqlserver-ex \
  --master-username admin \
  --master-user-password <secure-password> \
  --allocated-storage 20 \
  --vpc-security-group-ids <security-group-id> \
  --db-subnet-group-name <subnet-group-name> \
  --backup-retention-period 7 \
  --multi-az

# Create RDS Proxy for connection pooling
aws rds create-db-proxy \
  --db-proxy-name bankapp-proxy \
  --engine-family SQLSERVER \
  --auth [{AuthScheme=SECRETS,SecretArn=<secret-arn>}] \
  --role-arn <iam-role-arn> \
  --vpc-subnet-ids <subnet-id-1> <subnet-id-2>
```

### 3. AWS Managed Microsoft AD

```bash
# Create Managed Microsoft AD
aws ds create-microsoft-ad \
  --name corp.example.com \
  --short-name CORP \
  --password <secure-password> \
  --vpc-settings VpcId=<vpc-id>,SubnetIds=<subnet-id-1>,<subnet-id-2> \
  --edition Standard
```

### 4. Amazon SQS Queues

```bash
# Create transfer audit queue
aws sqs create-queue \
  --queue-name transfer-audit \
  --attributes VisibilityTimeout=30,MessageRetentionPeriod=345600

# Create customer info queue
aws sqs create-queue \
  --queue-name customer-info \
  --attributes VisibilityTimeout=30,MessageRetentionPeriod=345600

# Create dead-letter queues
aws sqs create-queue --queue-name transfer-audit-dlq
aws sqs create-queue --queue-name customer-info-dlq
```

### 5. Amazon S3 Buckets

```bash
# Create customer logs bucket
aws s3 mb s3://bank-customer-logs-<account-id>
aws s3api put-bucket-encryption \
  --bucket bank-customer-logs-<account-id> \
  --server-side-encryption-configuration '{"Rules":[{"ApplyServerSideEncryptionByDefault":{"SSEAlgorithm":"AES256"}}]}'

# Create deposit cache bucket
aws s3 mb s3://bank-deposit-cache-<account-id>
aws s3api put-bucket-encryption \
  --bucket bank-deposit-cache-<account-id> \
  --server-side-encryption-configuration '{"Rules":[{"ApplyServerSideEncryptionByDefault":{"SSEAlgorithm":"AES256"}}]}'

# Enable versioning
aws s3api put-bucket-versioning \
  --bucket bank-customer-logs-<account-id> \
  --versioning-configuration Status=Enabled
```

### 6. Amazon EFS for Shared Storage

```bash
# Create EFS file system
aws efs create-file-system \
  --performance-mode generalPurpose \
  --throughput-mode bursting \
  --encrypted \
  --tags Key=Name,Value=BankApp-AuditLogs

# Create mount targets in each subnet
aws efs create-mount-target \
  --file-system-id <fs-id> \
  --subnet-id <subnet-id-1> \
  --security-groups <security-group-id>

aws efs create-mount-target \
  --file-system-id <fs-id> \
  --subnet-id <subnet-id-2> \
  --security-groups <security-group-id>
```

### 7. AWS Systems Manager Parameter Store

```bash
# Store configuration parameters
aws ssm put-parameter \
  --name /BankApp/CustomerDashboard/Settings \
  --value '{"theme":"default","timeout":300}' \
  --type SecureString \
  --key-id alias/aws/ssm

# Store database connection string
aws ssm put-parameter \
  --name /BankApp/Database/ConnectionString \
  --value "Server=<rds-proxy-endpoint>;Database=BankDB;User Id=admin;Password=<password>;" \
  --type SecureString \
  --key-id alias/aws/ssm
```

## IAM Configuration

### 1. Create IAM Role for ECS/EKS Tasks

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:GetObject",
        "s3:PutObject",
        "s3:DeleteObject",
        "s3:ListBucket"
      ],
      "Resource": [
        "arn:aws:s3:::bank-customer-logs-*/*",
        "arn:aws:s3:::bank-deposit-cache-*/*"
      ]
    },
    {
      "Effect": "Allow",
      "Action": [
        "sqs:SendMessage",
        "sqs:ReceiveMessage",
        "sqs:DeleteMessage",
        "sqs:GetQueueUrl"
      ],
      "Resource": [
        "arn:aws:sqs:*:*:transfer-audit",
        "arn:aws:sqs:*:*:customer-info"
      ]
    },
    {
      "Effect": "Allow",
      "Action": [
        "ssm:GetParameter",
        "ssm:GetParameters",
        "ssm:GetParametersByPath"
      ],
      "Resource": "arn:aws:ssm:*:*:parameter/BankApp/*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "rds-db:connect"
      ],
      "Resource": "arn:aws:rds-db:*:*:dbuser:*/*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:PutLogEvents"
      ],
      "Resource": "arn:aws:logs:*:*:*"
    }
  ]
}
```

### 2. Create IAM Role for RDS Proxy

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "secretsmanager:GetSecretValue"
      ],
      "Resource": "arn:aws:secretsmanager:*:*:secret:rds-db-credentials-*"
    }
  ]
}
```

## Security Groups

### 1. Application Security Group

```bash
aws ec2 create-security-group \
  --group-name bankapp-sg \
  --description "Security group for Bank Management System" \
  --vpc-id <vpc-id>

# Allow inbound HTTP/HTTPS
aws ec2 authorize-security-group-ingress \
  --group-id <sg-id> \
  --protocol tcp \
  --port 80 \
  --cidr 0.0.0.0/0

aws ec2 authorize-security-group-ingress \
  --group-id <sg-id> \
  --protocol tcp \
  --port 443 \
  --cidr 0.0.0.0/0
```

### 2. Database Security Group

```bash
aws ec2 create-security-group \
  --group-name bankapp-db-sg \
  --description "Security group for RDS database" \
  --vpc-id <vpc-id>

# Allow inbound from application security group
aws ec2 authorize-security-group-ingress \
  --group-id <db-sg-id> \
  --protocol tcp \
  --port 1433 \
  --source-group <app-sg-id>
```

### 3. LDAP Security Group

```bash
aws ec2 create-security-group \
  --group-name bankapp-ldap-sg \
  --description "Security group for LDAP" \
  --vpc-id <vpc-id>

# Allow LDAP from application security group
aws ec2 authorize-security-group-ingress \
  --group-id <ldap-sg-id> \
  --protocol tcp \
  --port 389 \
  --source-group <app-sg-id>

# Allow LDAPS
aws ec2 authorize-security-group-ingress \
  --group-id <ldap-sg-id> \
  --protocol tcp \
  --port 636 \
  --source-group <app-sg-id>
```

### 4. EFS Security Group

```bash
aws ec2 create-security-group \
  --group-name bankapp-efs-sg \
  --description "Security group for EFS" \
  --vpc-id <vpc-id>

# Allow NFS from application security group
aws ec2 authorize-security-group-ingress \
  --group-id <efs-sg-id> \
  --protocol tcp \
  --port 2049 \
  --source-group <app-sg-id>
```

## Environment Variables Configuration

Create a file `env-config.json` for container deployment:

```json
{
  "environment": [
    {
      "name": "DATABASE_CONNECTION_STRING",
      "value": "Server=<rds-proxy-endpoint>;Database=BankDB;User Id=admin;Password=<password>;"
    },
    {
      "name": "LDAP_SERVER",
      "value": "<managed-ad-dns-name>"
    },
    {
      "name": "LDAP_PORT",
      "value": "389"
    },
    {
      "name": "LDAP_BASE_DN",
      "value": "dc=corp,dc=example,dc=com"
    },
    {
      "name": "AUDIT_LOG_PATH",
      "value": "/mnt/efs/audit"
    },
    {
      "name": "TRANSFER_AUDIT_QUEUE",
      "value": "transfer-audit"
    },
    {
      "name": "CUSTOMER_INFO_QUEUE",
      "value": "customer-info"
    },
    {
      "name": "CUSTOMER_LOGS_BUCKET",
      "value": "bank-customer-logs-<account-id>"
    },
    {
      "name": "DEPOSIT_CACHE_BUCKET",
      "value": "bank-deposit-cache-<account-id>"
    },
    {
      "name": "AWS_REGION",
      "value": "us-east-1"
    }
  ]
}
```

## Database Migration

### 1. Create Database Schema

```sql
-- Connect to RDS instance
-- Create database
CREATE DATABASE BankDB;
GO

USE BankDB;
GO

-- Create tables (example)
CREATE TABLE Customers (
    CustomerId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Email NVARCHAR(255),
    CreatedDate DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Email NVARCHAR(255),
    Role NVARCHAR(50),
    CreatedDate DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE Accounts (
    AccountId INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT FOREIGN KEY REFERENCES Customers(CustomerId),
    AccountNumber NVARCHAR(20) NOT NULL UNIQUE,
    Balance DECIMAL(18,2) DEFAULT 0,
    AccountType NVARCHAR(50),
    CreatedDate DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE Transactions (
    TransactionId INT PRIMARY KEY IDENTITY(1,1),
    AccountId INT FOREIGN KEY REFERENCES Accounts(AccountId),
    TransactionType NVARCHAR(50),
    Amount DECIMAL(18,2),
    TransactionDate DATETIME2 DEFAULT GETUTCDATE(),
    Description NVARCHAR(500)
);
```

### 2. Migrate Existing Data

```bash
# Export data from existing database
# Import to RDS using SQL Server Management Studio or sqlcmd
```

## Monitoring Setup

### 1. CloudWatch Log Groups

```bash
# Create log group
aws logs create-log-group --log-group-name /aws/bankapp/application

# Set retention policy
aws logs put-retention-policy \
  --log-group-name /aws/bankapp/application \
  --retention-in-days 30
```

### 2. CloudWatch Alarms

```bash
# Create alarm for high error rate
aws cloudwatch put-metric-alarm \
  --alarm-name bankapp-high-error-rate \
  --alarm-description "Alert when error rate is high" \
  --metric-name Errors \
  --namespace AWS/ApplicationELB \
  --statistic Sum \
  --period 300 \
  --threshold 10 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 2

# Create alarm for database connections
aws cloudwatch put-metric-alarm \
  --alarm-name bankapp-db-connections \
  --alarm-description "Alert when database connections are high" \
  --metric-name DatabaseConnections \
  --namespace AWS/RDS \
  --statistic Average \
  --period 300 \
  --threshold 80 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 2
```

## Deployment Checklist

- [ ] VPC and subnets created
- [ ] Security groups configured
- [ ] RDS instance and RDS Proxy created
- [ ] AWS Managed Microsoft AD deployed
- [ ] SQS queues created
- [ ] S3 buckets created with encryption
- [ ] EFS file system created and mounted
- [ ] Parameter Store values configured
- [ ] IAM roles and policies created
- [ ] Database schema migrated
- [ ] Environment variables configured
- [ ] CloudWatch logging enabled
- [ ] CloudWatch alarms configured
- [ ] Application containerized (separate workflow)
- [ ] Container deployed to ECS/EKS (separate workflow)

## Testing

### 1. Test Database Connectivity

```bash
# Test RDS Proxy connection
sqlcmd -S <rds-proxy-endpoint> -U admin -P <password> -Q "SELECT @@VERSION"
```

### 2. Test LDAP Authentication

```bash
# Test LDAP bind
ldapsearch -x -H ldap://<managed-ad-dns>:389 -D "cn=admin,dc=corp,dc=example,dc=com" -w <password> -b "dc=corp,dc=example,dc=com"
```

### 3. Test SQS Queues

```bash
# Send test message
aws sqs send-message \
  --queue-url <queue-url> \
  --message-body "Test message"

# Receive message
aws sqs receive-message --queue-url <queue-url>
```

### 4. Test S3 Access

```bash
# Upload test file
echo "test" > test.txt
aws s3 cp test.txt s3://bank-customer-logs-<account-id>/test.txt

# Download test file
aws s3 cp s3://bank-customer-logs-<account-id>/test.txt downloaded.txt
```

### 5. Test Parameter Store

```bash
# Get parameter
aws ssm get-parameter \
  --name /BankApp/CustomerDashboard/Settings \
  --with-decryption
```

## Troubleshooting

### Database Connection Issues
- Verify RDS Proxy endpoint is correct
- Check security group rules
- Verify IAM role has rds-db:connect permission
- Check connection string format

### LDAP Authentication Issues
- Verify Managed AD DNS name
- Check security group allows port 389/636
- Verify LDAP base DN is correct
- Test with ldapsearch command

### SQS Message Delivery Issues
- Check IAM role has SQS permissions
- Verify queue URLs are correct
- Check dead-letter queue for failed messages
- Review CloudWatch Logs for errors

### S3 Access Issues
- Verify IAM role has S3 permissions
- Check bucket names are correct
- Verify bucket encryption settings
- Review CloudWatch Logs for errors

### EFS Mount Issues
- Verify EFS mount targets are in correct subnets
- Check security group allows NFS (port 2049)
- Verify mount path is correct in container
- Check EFS file system is available

## Cost Optimization

1. **RDS**: Use Reserved Instances for production
2. **EFS**: Use Infrequent Access storage class for old logs
3. **S3**: Configure lifecycle policies to move old data to Glacier
4. **SQS**: Use long polling to reduce API calls
5. **CloudWatch**: Set appropriate log retention periods

## Security Best Practices

1. Enable AWS CloudTrail for audit logging
2. Use AWS Secrets Manager for sensitive credentials
3. Enable VPC Flow Logs for network monitoring
4. Implement least privilege IAM policies
5. Enable MFA for AWS console access
6. Use AWS WAF for application protection
7. Enable GuardDuty for threat detection
8. Regularly rotate credentials and keys

## Backup and Disaster Recovery

1. **RDS**: Automated backups enabled (7-day retention)
2. **S3**: Versioning enabled for data recovery
3. **EFS**: AWS Backup for file system snapshots
4. **Parameter Store**: Version history maintained
5. **Multi-AZ**: Deploy across multiple availability zones

## Support and Maintenance

- Monitor CloudWatch dashboards daily
- Review CloudWatch Alarms
- Check SQS dead-letter queues
- Review application logs
- Perform regular security updates
- Test disaster recovery procedures quarterly

## Additional Resources

- [AWS Well-Architected Framework](https://aws.amazon.com/architecture/well-architected/)
- [AWS Security Best Practices](https://aws.amazon.com/security/best-practices/)
- [.NET on AWS](https://aws.amazon.com/developer/language/net/)
- [AWS RDS Proxy](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/rds-proxy.html)
- [AWS Managed Microsoft AD](https://docs.aws.amazon.com/directoryservice/latest/admin-guide/directory_microsoft_ad.html)
