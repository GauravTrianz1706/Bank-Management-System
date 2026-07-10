using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// Blocker-3 & Blocker-4 (cr-dotnet-0043): Removed System.Messaging (MSMQ) dependency.
// Replaced MessageQueue with Google Cloud Pub/Sub PublisherClient for durable,
// scalable, at-least-once asynchronous message delivery on GCP.

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Tansfer : Form
    {
        // Blocker-3 & Blocker-4 (cr-dotnet-0043): Replace MSMQ MessageQueue with
        // Google Cloud Pub/Sub PublisherClient.
        // Topic name is configured via the PUBSUB_TRANSFER_AUDIT_TOPIC environment variable
        // (e.g., "projects/my-project/topics/transfer-audit") injected by GKE ConfigMap.
        private readonly TopicName _transferAuditTopic;

        private int[] recentTransfers = new[] { 100, 200 };
        public int[] RecentTransfers => recentTransfers;

        public Tansfer(PersonModel customer)
        {
            InitializeComponent();

            // Blocker-3 (cr-dotnet-0043): Resolve Pub/Sub topic from environment variable.
            string topicId =
                Environment.GetEnvironmentVariable("PUBSUB_TRANSFER_AUDIT_TOPIC")
                ?? "transfer-audit";
            string projectId =
                Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT")
                ?? throw new InvalidOperationException(
                    "GOOGLE_CLOUD_PROJECT environment variable is not set.");

            _transferAuditTopic = new TopicName(projectId, topicId);

            // Blocker-4 (cr-dotnet-0043): Replace queue.Send() with
            // PublisherClient.PublishAsync() for cloud-native async messaging.
            PublishTransferAuditAsync("Transfer initiated").GetAwaiter().GetResult();
        }

        // Blocker-4 (cr-dotnet-0043): Async publish method using Google Cloud Pub/Sub
        // PublisherClient, replacing the synchronous MSMQ MessageQueue.Send() call.
        private async Task PublishTransferAuditAsync(string message)
        {
            PublisherClient publisher = await PublisherClient.CreateAsync(_transferAuditTopic);
            ByteString data = ByteString.CopyFromUtf8(message);
            PubsubMessage pubsubMessage = new PubsubMessage { Data = data };
            await publisher.PublishAsync(pubsubMessage);
        }
    }
}
