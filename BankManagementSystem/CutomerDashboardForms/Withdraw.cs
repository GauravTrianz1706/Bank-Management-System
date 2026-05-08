using System.Diagnostics;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Withdraw : Form
    {
        public Withdraw()
        {
            InitializeComponent();

            
            var sessionId = Process.GetCurrentProcess().SessionId;
        }

        
        private struct WithdrawalToken
        {
            public int Code;
        }

        private void AmountTextBox_Enter(object? sender, System.EventArgs e)
        {
            // Placeholder for AmountTextBox_Enter event handler
        }

        private void AmountTextBox_Leave(object? sender, System.EventArgs e)
        {
            // Placeholder for AmountTextBox_Leave event handler
        }

        private void WithdrawBtn_Click(object? sender, System.EventArgs e)
        {
            // Placeholder for WithdrawBtn_Click event handler
        }
    }
}
