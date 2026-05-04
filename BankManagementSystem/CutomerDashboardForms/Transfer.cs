using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Tansfer : Form
    {
        // Fixed: Replaced System.Messaging.MessageQueue with logging
        // For cloud deployment, consider using AWS SQS, Azure Service Bus, or RabbitMQ
        private readonly PersonModel _customer;
        
        private int[] recentTransfers = new[] { 100, 200 };
        public int[] RecentTransfers => recentTransfers;

        public Tansfer(PersonModel customer)
        {
            _customer = customer;
            InitializeComponent();
            
            // Log transfer initiation instead of using MSMQ
            LogTransferEvent("Transfer initiated");
        }

        private void LogTransferEvent(string message)
        {
            // Fixed: Use console logging instead of MSMQ
            // In production, this should be replaced with proper logging framework (log4net, Serilog)
            // or cloud-native messaging service (AWS SQS, Azure Service Bus)
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Transfer Event: {message}");
        }
    }
}
