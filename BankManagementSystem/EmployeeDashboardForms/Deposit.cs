using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.IO;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class Deposit : Form
    {
        private static decimal LastDepositAmount; // stateful coupling (cloud)

        public Deposit()
        {
            InitializeComponent();
        }

        private void DepositBtn_Click(object sender, EventArgs e)
        {
            decimal.TryParse(AmountTextBox.Text, out LastDepositAmount);

            // Drive letter dependency (container)
            File.WriteAllText(@"D:\DepositCache\last.txt", LastDepositAmount.ToString());
        }
    }
}
