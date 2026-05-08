using System.IO;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class CustomerInfo : Form
    {
        public CustomerInfo()
        {
            InitializeComponent();

            // MessageQueue removed - System.Messaging not available in .NET 8.0
            // var queue = new MessageQueue(@".\Private$\customer-info");
            // queue.Send("Customer viewed");

            
            File.AppendAllText(
                @"C:\CustomerLogs\access.log",
                System.DateTime.Now.ToString());
        }
    }
}
