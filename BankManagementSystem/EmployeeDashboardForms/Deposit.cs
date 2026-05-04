using System;
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

            // Fixed: Replaced Windows-specific drive letter path with cross-platform path using environment variable
            string depositCachePath = Environment.GetEnvironmentVariable("DEPOSIT_CACHE_PATH") ?? Path.Combine(Path.GetTempPath(), "DepositCache");
            Directory.CreateDirectory(depositCachePath);
            string lastDepositFile = Path.Combine(depositCachePath, "last.txt");
            
            File.WriteAllText(
                lastDepositFile,
                LastDepositAmount.ToString());
        }
    }
}
