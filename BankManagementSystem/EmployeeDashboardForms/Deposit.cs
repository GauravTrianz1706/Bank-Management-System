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

            // CONTAINERIZATION FIX: Replaced hardcoded Windows drive letter path (D:\DepositCache\last.txt)
            // with cross-platform path using environment variable and Path.Combine
            // Blocker IDs: blocker-3 (cz-dotnet-0001 - Windows-Specific Paths),
            //              blocker-8 (cz-dotnet-0006 - Drive Letter Dependencies)
            // In production, use Azure Redis Cache or similar for temporary data storage
            string depositCachePath = Environment.GetEnvironmentVariable("DEPOSIT_CACHE_PATH") 
                ?? Path.Combine(Path.GetTempPath(), "DepositCache");
            
            // Ensure directory exists
            if (!Directory.Exists(depositCachePath))
            {
                Directory.CreateDirectory(depositCachePath);
            }

            File.WriteAllText(
                Path.Combine(depositCachePath, "last.txt"),
                LastDepositAmount.ToString());
        }
    }
}
