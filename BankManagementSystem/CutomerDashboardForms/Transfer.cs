using Azure.Messaging.ServiceBus;
using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Tansfer : Form
    {
        // Replaced System.Messaging.MessageQueue (MSMQ) with Azure Service Bus ServiceBusClient.
        // Azure Service Bus provides distributed queues with at-least-once delivery,
        // dead-letter queues, and message sessions, matching MSMQ semantics while
        // adding cloud-scale durability and geographic replication.
        private readonly ServiceBusClient serviceBusClient;
        private readonly ServiceBusSender serviceBusSender;

        private int[] recentTransfers = new[] { 100, 200 };
        public int[] RecentTransfers => recentTransfers;

        public Tansfer(PersonModel customer)
        {
            InitializeComponent();

            // Connection string is read from environment variable for cloud-native configuration.
            string serviceBusConnectionString = Environment.GetEnvironmentVariable("AZURE_SERVICE_BUS_CONNECTION_STRING")
                ?? throw new InvalidOperationException(
                    "Azure Service Bus connection string is not configured. " +
                    "Set the 'AZURE_SERVICE_BUS_CONNECTION_STRING' environment variable.");

            serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
            serviceBusSender = serviceBusClient.CreateSender("transfer-audit");

            // Send message asynchronously to Azure Service Bus (fire-and-forget for UI init).
            _ = serviceBusSender.SendMessageAsync(new ServiceBusMessage("Transfer initiated"));
        }
    }
}
