using BankDatabaseAccess.DatabaseOperation;
using System.Messaging;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class CustomerInfo : Form
    {
        // MSMQ usage (cloud)
        private readonly MessageQueue queue =
            new MessageQueue(@".\Private$\customer-info");

        public CustomerInfo()
        {
            InitializeComponent();
            queue.Send("viewed");
        }
    }
}
