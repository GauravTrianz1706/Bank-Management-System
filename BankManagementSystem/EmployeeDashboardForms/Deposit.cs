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

            // Cache the last deposit amount
            try
            {
                var cacheDir = @"D:\DepositCache";
                if (!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                }
                File.WriteAllText(
                    Path.Combine(cacheDir, "last.txt"),
                    LastDepositAmount.ToString());
            }
            catch
            {
                // Ignore caching errors
            }
        }
    }
}
