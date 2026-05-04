using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class CustomerInfo : Form
    {
        // Blocker 5 & 6 Fix: Replace MSMQ with Amazon SQS
        private readonly IAmazonSQS sqsClient;
        private readonly string queueUrl;

        // Blocker 10, 11, 12 Fix: Replace local file operations with Amazon S3
        private readonly IAmazonS3 s3Client;
        private readonly string bucketName;

        public CustomerInfo()
        {
            InitializeComponent();

            // Blocker 5 & 6 Fix: Initialize Amazon SQS client
            sqsClient = new AmazonSQSClient();
            string queueName = Environment.GetEnvironmentVariable("CUSTOMER_INFO_QUEUE") ?? "customer-info";
            queueUrl = GetQueueUrlAsync(queueName).Result;
            
            // Send message to SQS instead of MSMQ
            SendAuditMessageAsync("Customer viewed").Wait();

            // Blocker 10, 11, 12 Fix: Initialize Amazon S3 client for file operations
            s3Client = new AmazonS3Client();
            bucketName = Environment.GetEnvironmentVariable("CUSTOMER_LOGS_BUCKET") ?? "bank-customer-logs";
            
            // Blocker 8 Fix: Replace hard-coded path with S3 key
            // Blocker 17 Fix: Replace DateTime.Now with DateTimeOffset.UtcNow for timezone consistency
            string logKey = "access-logs/access.log";
            string logEntry = DateTimeOffset.UtcNow.ToString("o"); // ISO 8601 UTC format
            
            // Write to S3 instead of local file system
            AppendToS3LogAsync(logKey, logEntry).Wait();
        }

        // Blocker 5 & 6 Fix: Get SQS queue URL
        private async Task<string> GetQueueUrlAsync(string queueName)
        {
            try
            {
                var request = new GetQueueUrlRequest
                {
                    QueueName = queueName
                };
                var response = await sqsClient.GetQueueUrlAsync(request);
                return response.QueueUrl;
            }
            catch (QueueDoesNotExistException)
            {
                // Create queue if it doesn't exist
                var createRequest = new CreateQueueRequest
                {
                    QueueName = queueName
                };
                var createResponse = await sqsClient.CreateQueueAsync(createRequest);
                return createResponse.QueueUrl;
            }
        }

        // Blocker 5 & 6 Fix: Send message to Amazon SQS
        private async Task SendAuditMessageAsync(string message)
        {
            try
            {
                var sendRequest = new SendMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageBody = message,
                    MessageAttributes = new System.Collections.Generic.Dictionary<string, MessageAttributeValue>
                    {
                        {
                            "Timestamp",
                            new MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = DateTimeOffset.UtcNow.ToString("o")
                            }
                        }
                    }
                };

                await sqsClient.SendMessageAsync(sendRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send message to SQS: {ex.Message}");
            }
        }

        // Blocker 10, 11, 12 Fix: Append log entry to S3 object
        private async Task AppendToS3LogAsync(string key, string content)
        {
            try
            {
                // Ensure bucket exists
                await EnsureBucketExistsAsync();

                // Read existing content if file exists
                string existingContent = "";
                try
                {
                    var getRequest = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key
                    };
                    using (var response = await s3Client.GetObjectAsync(getRequest))
                    using (var reader = new StreamReader(response.ResponseStream))
                    {
                        existingContent = await reader.ReadToEndAsync();
                    }
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // File doesn't exist yet, that's okay
                }

                // Append new content
                string newContent = existingContent + content + Environment.NewLine;

                // Write back to S3
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    ContentBody = newContent,
                    ContentType = "text/plain"
                };

                await s3Client.PutObjectAsync(putRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to S3: {ex.Message}");
            }
        }

        // Ensure S3 bucket exists
        private async Task EnsureBucketExistsAsync()
        {
            try
            {
                await s3Client.PutBucketAsync(new PutBucketRequest
                {
                    BucketName = bucketName,
                    UseClientRegion = true
                });
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketAlreadyOwnedByYou")
            {
                // Bucket already exists, that's fine
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sqsClient?.Dispose();
                s3Client?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
