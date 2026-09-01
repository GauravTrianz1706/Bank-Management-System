using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class CustomerInfo : Form
    {
        public CustomerInfo()
        {
            InitializeComponent();

            // Replaced System.Messaging.MessageQueue (MSMQ) with Azure Service Bus ServiceBusClient.
            // Azure Service Bus provides distributed queues with at-least-once delivery,
            // dead-letter queues, and message sessions, matching MSMQ semantics while
            // adding cloud-scale durability and geographic replication.
            string serviceBusConnectionString = Environment.GetEnvironmentVariable("AZURE_SERVICE_BUS_CONNECTION_STRING")
                ?? throw new InvalidOperationException(
                    "Azure Service Bus connection string is not configured. " +
                    "Set the 'AZURE_SERVICE_BUS_CONNECTION_STRING' environment variable.");

            var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
            var serviceBusSender = serviceBusClient.CreateSender("customer-info");
            _ = serviceBusSender.SendMessageAsync(new ServiceBusMessage("Customer viewed"));

            // Replaced File.AppendAllText with hard-coded Windows path (C:\CustomerLogs\access.log)
            // with Azure Blob Storage upload using BlobClient.UploadAsync.
            // Also replaced DateTime.Now with DateTimeOffset.UtcNow for timezone-consistent
            // logging across globally distributed Azure regions.
            // The storage account URL and container name are read from environment variables
            // for cloud-native, credential-free access via Workload Identity.
            string storageAccountUrl = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_URL")
                ?? throw new InvalidOperationException(
                    "Azure Storage account URL is not configured. " +
                    "Set the 'AZURE_STORAGE_ACCOUNT_URL' environment variable.");

            string containerName = Environment.GetEnvironmentVariable("CUSTOMER_LOGS_CONTAINER")
                ?? "customer-logs";

            // Use DateTimeOffset.UtcNow for timezone-consistent timestamps across Azure regions.
            string logEntry = DateTimeOffset.UtcNow.ToString("o");
            string blobName = $"access-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.log";

            var blobServiceClient = new BlobServiceClient(new Uri(storageAccountUrl),
                new Azure.Identity.DefaultAzureCredential());
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(logEntry));
            _ = blobClient.UploadAsync(stream, overwrite: true);
        }
    }
}
