using System.IO;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class CustomerInfo : Form
    {
        public CustomerInfo()
        {
            InitializeComponent();

            // Note: System.Messaging is not available in .NET 8
            // Message queue functionality has been removed
            // var queue = new MessageQueue(@".\Private$\customer-info");
            // queue.Send("Customer viewed");

            // Log customer access
            try
            {
                var logDir = @"C:\CustomerLogs";
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                File.AppendAllText(
                    Path.Combine(logDir, "access.log"),
                    System.DateTime.Now.ToString() + System.Environment.NewLine);
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}
