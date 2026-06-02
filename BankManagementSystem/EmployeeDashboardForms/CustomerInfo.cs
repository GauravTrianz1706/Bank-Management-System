using System.IO;
using System.Windows.Forms;

// NOTE: System.Messaging (MSMQ) is NOT available in .NET 8.
// The MessageQueue type was removed from .NET Core / .NET 5+.
// The audit queue usage has been replaced with a file-based log using
// AppContext.BaseDirectory so the path is not hard-coded to C:\.
// In production, replace with a supported messaging library.

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class CustomerInfo : Form
    {
        public CustomerInfo()
        {
            InitializeComponent();

            // REMOVED: MessageQueue (System.Messaging) is not available in .NET 8.
            // Replace with a supported messaging solution (e.g., RabbitMQ, Azure Service Bus).
            // var queue = new MessageQueue(@".\Private$\customer-info");
            // queue.Send("Customer viewed");

            // Replaced hard-coded absolute path (C:\CustomerLogs\access.log) with
            // a path relative to the application base directory.
            string logDir = System.IO.Path.Combine(
                AppContext.BaseDirectory, "CustomerLogs");
            System.IO.Directory.CreateDirectory(logDir);
            File.AppendAllText(
                System.IO.Path.Combine(logDir, "access.log"),
                System.DateTime.UtcNow.ToString("O") + System.Environment.NewLine);
        }
    }
}
