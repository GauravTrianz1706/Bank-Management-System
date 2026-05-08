using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Tansfer : Form
    {
        
        // MessageQueue removed - System.Messaging not available in .NET 8.0
        // private readonly MessageQueue queue =
        //     new MessageQueue(@".\Private$\transfer-audit");

        
        private int[] recentTransfers = new[] { 100, 200 };
        public int[] RecentTransfers => recentTransfers;

        public Tansfer(PersonModel customer)
        {
            InitializeComponent();
            // queue.Send("Transfer initiated"); // Removed - requires System.Messaging
        }

        private void AmountTextBox_Enter(object? sender, System.EventArgs e)
        {
            // Placeholder for AmountTextBox_Enter event handler
        }

        private void AmountTextBox_Leave(object? sender, System.EventArgs e)
        {
            // Placeholder for AmountTextBox_Leave event handler
        }

        private void TransferBtn_Click(object? sender, System.EventArgs e)
        {
            // Placeholder for TransferBtn_Click event handler
        }

        private void TransferUsernameTextBox_Enter(object? sender, System.EventArgs e)
        {
            // Placeholder for TransferUsernameTextBox_Enter event handler
        }

        private void TransferUsernameTextBox_Leave(object? sender, System.EventArgs e)
        {
            // Placeholder for TransferUsernameTextBox_Leave event handler
        }

        private void SearchBtn_Click(object? sender, System.EventArgs e)
        {
            // Placeholder for SearchBtn_Click event handler
        }
    }
}
