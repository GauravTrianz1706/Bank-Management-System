using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Tansfer : Form
    {
        // Blocker 3 & 4 Fix: Replace MSMQ with Amazon SQS
        private readonly IAmazonSQS sqsClient;
        private readonly string queueUrl;

        // Instance state instead of static
        private int[] recentTransfers = new[] { 100, 200 };
        public int[] RecentTransfers => recentTransfers;

        public Tansfer(PersonModel customer)
        {
            InitializeComponent();
            
            // Blocker 3 & 4 Fix: Initialize Amazon SQS client
            sqsClient = new AmazonSQSClient();
            
            // Get queue URL from environment variable or use default
            string queueName = Environment.GetEnvironmentVariable("TRANSFER_AUDIT_QUEUE") ?? "transfer-audit";
            queueUrl = GetQueueUrlAsync(queueName).Result;
            
            // Send message to SQS instead of MSMQ
            SendAuditMessageAsync("Transfer initiated").Wait();
        }

        // Blocker 3 & 4 Fix: Get SQS queue URL
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

        // Blocker 3 & 4 Fix: Send message to Amazon SQS
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
                // Log error
                Console.WriteLine($"Failed to send message to SQS: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sqsClient?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
