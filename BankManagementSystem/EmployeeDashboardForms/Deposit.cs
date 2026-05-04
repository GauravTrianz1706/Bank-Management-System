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

            // Fixed: Replaced hardcoded Windows drive letter path with environment variable
            string depositCachePath = Environment.GetEnvironmentVariable("DEPOSIT_CACHE_PATH") ?? "/app/cache/last.txt";
            File.WriteAllText(
                depositCachePath,
                LastDepositAmount.ToString());
        }
    }
}
