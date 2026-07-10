using Google.Cloud.PubSub.V1;
using Google.Cloud.Storage.V1;
using Google.Protobuf;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// Blocker-5 & Blocker-6 (cr-dotnet-0043): Removed System.Messaging (MSMQ) dependency.
//   Replaced MessageQueue with Google Cloud Pub/Sub PublisherClient.
// Blocker-8  (cr-dotnet-0001): Removed hard-coded Windows path C:\CustomerLogs\access.log.
//   Replaced with Cloud Storage bucket URI configured via environment variables.
// Blocker-10 (cr-dotnet-0002): Replaced File.AppendAllText() local write with
//   StorageClient.UploadObjectAsync() for durable GCS persistence.
// Blocker-12 (cr-dotnet-0003): Replaced System.IO.File API with Google Cloud Storage
//   StorageClient methods for cloud-native object storage.
// Blocker-16 (cr-dotnet-0121): Replaced DateTime.Now with DateTime.UtcNow for
//   timezone-consistent timestamps across distributed GKE pods.

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class CustomerInfo : Form
    {
        public CustomerInfo()
        {
            InitializeComponent();

            // Blocker-5 & Blocker-6 (cr-dotnet-0043): Replace MSMQ MessageQueue with
            // Google Cloud Pub/Sub PublisherClient for cloud-native async messaging.
            PublishCustomerViewedAsync("Customer viewed").GetAwaiter().GetResult();

            // Blocker-8  (cr-dotnet-0001): Replace hard-coded Windows path with GCS URI.
            // Blocker-10 (cr-dotnet-0002): Replace File.AppendAllText() with GCS upload.
            // Blocker-12 (cr-dotnet-0003): Replace System.IO.File API with StorageClient.
            // Blocker-16 (cr-dotnet-0121): Use DateTime.UtcNow instead of DateTime.Now.
            UploadAccessLogAsync(DateTime.UtcNow.ToString("o")).GetAwaiter().GetResult();
        }

        // Blocker-5 & Blocker-6 (cr-dotnet-0043): Async publish using Google Cloud Pub/Sub,
        // replacing synchronous MSMQ MessageQueue.Send() call.
        private async Task PublishCustomerViewedAsync(string message)
        {
            string projectId =
                Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT")
                ?? throw new InvalidOperationException(
                    "GOOGLE_CLOUD_PROJECT environment variable is not set.");
            string topicId =
                Environment.GetEnvironmentVariable("PUBSUB_CUSTOMER_INFO_TOPIC")
                ?? "customer-info";

            TopicName topicName = new TopicName(projectId, topicId);
            PublisherClient publisher = await PublisherClient.CreateAsync(topicName);
            ByteString data = ByteString.CopyFromUtf8(message);
            PubsubMessage pubsubMessage = new PubsubMessage { Data = data };
            await publisher.PublishAsync(pubsubMessage);
        }

        // Blocker-8, Blocker-10, Blocker-12 (cr-dotnet-0001/0002/0003):
        // Upload access log entry to Google Cloud Storage instead of writing to a
        // local Windows file path. Workload Identity Federation grants pod-level
        // access to Cloud Storage without JSON key files.
        private async Task UploadAccessLogAsync(string logEntry)
        {
            string bucketName =
                Environment.GetEnvironmentVariable("GCS_CUSTOMER_LOGS_BUCKET")
                ?? throw new InvalidOperationException(
                    "GCS_CUSTOMER_LOGS_BUCKET environment variable is not set.");
            string objectName =
                Environment.GetEnvironmentVariable("GCS_CUSTOMER_LOGS_OBJECT")
                ?? "access.log";

            StorageClient storageClient = await StorageClient.CreateAsync();
            byte[] logBytes = Encoding.UTF8.GetBytes(logEntry + Environment.NewLine);
            using (MemoryStream stream = new MemoryStream(logBytes))
            {
                await storageClient.UploadObjectAsync(
                    bucketName,
                    objectName,
                    "text/plain",
                    stream);
            }
        }
    }
}
