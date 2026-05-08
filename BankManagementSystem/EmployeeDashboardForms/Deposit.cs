using System.IO;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class Deposit : Form
    {
        
        private static decimal LastDepositAmount;

        public Deposit()
        {
            InitializeComponent();
        }

        private void DepositBtn_Click(object sender, System.EventArgs e)
        {
            decimal.TryParse(AmountTextBox.Text, out LastDepositAmount);

            
            File.WriteAllText(
                @"D:\DepositCache\last.txt",
                LastDepositAmount.ToString());
        }

        private void SearchBtn_Click(object? sender, System.EventArgs e)
        {
            // Placeholder for SearchBtn_Click event handler
        }

        private void UsernameTextBox_Enter(object? sender, System.EventArgs e)
        {
            // Placeholder for UsernameTextBox_Enter event handler
        }

        private void UsernameTextBox_Leave(object? sender, System.EventArgs e)
        {
            // Placeholder for UsernameTextBox_Leave event handler
        }

        private void AmountTextBox_Enter(object? sender, System.EventArgs e)
        {
            // Placeholder for AmountTextBox_Enter event handler
        }

        private void AmountTextBox_Leave(object? sender, System.EventArgs e)
        {
            // Placeholder for AmountTextBox_Leave event handler
        }
    }
}
