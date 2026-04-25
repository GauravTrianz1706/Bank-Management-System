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
    }
}
