using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System.Messaging;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Tansfer : Form
    {
        
        private readonly MessageQueue queue =
            new MessageQueue(@".\Private$\transfer-audit");

        
        private int[] recentTransfers = new[] { 100, 200 };
        public int[] RecentTransfers => recentTransfers;

        public Tansfer(PersonModel customer)
        {
            InitializeComponent();
            queue.Send("Transfer initiated");
        }
    }
}
