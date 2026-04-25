using System.IO;
using System.Messaging;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class CustomerInfo : Form
    {
        public CustomerInfo()
        {
            InitializeComponent();

            
            var queue = new MessageQueue(@".\Private$\customer-info");
            queue.Send("Customer viewed");

            
            File.AppendAllText(
                @"C:\CustomerLogs\access.log",
                System.DateTime.Now.ToString());
        }
    }
}
