using System;
using System.IO;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class CustomerInfo : Form
    {
        public CustomerInfo()
        {
            InitializeComponent();

            // Fixed: Replaced System.Messaging.MessageQueue with logging
            // For cloud deployment, consider using AWS SQS, Azure Service Bus, or RabbitMQ
            LogCustomerEvent("Customer viewed");

            // Fixed: Replaced Windows-specific path with cross-platform path using environment variable
            string customerLogsPath = Environment.GetEnvironmentVariable("CUSTOMER_LOGS_PATH") ?? Path.Combine(Path.GetTempPath(), "CustomerLogs");
            Directory.CreateDirectory(customerLogsPath);
            string accessLogFile = Path.Combine(customerLogsPath, "access.log");
            
            File.AppendAllText(
                accessLogFile,
                DateTime.Now.ToString());
        }

        private void LogCustomerEvent(string message)
        {
            // Fixed: Use console logging instead of MSMQ
            // In production, this should be replaced with proper logging framework (log4net, Serilog)
            // or cloud-native messaging service (AWS SQS, Azure Service Bus)
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Customer Event: {message}");
        }
    }
}
