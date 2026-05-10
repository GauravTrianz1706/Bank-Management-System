using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class Deposit : Form
    {
        private static decimal LastDepositAmount;
        private PersonModel? targetCustomer;

        private const string UsernamePlaceholder = "Customer Username";
        private const string AmountPlaceholder = "Enter Deposit Amount";

        public Deposit()
        {
            InitializeComponent();
        }

        private void UsernameTextBox_Enter(object sender, EventArgs e)
        {
            UILogics.EnterUpdate(UsernameTextBox, UsernamePlaceholder);
        }

        private void UsernameTextBox_Leave(object sender, EventArgs e)
        {
            UILogics.LeaveUpdate(UsernameTextBox, UsernamePlaceholder);
        }

        private void AmountTextBox_Enter(object sender, EventArgs e)
        {
            UILogics.EnterUpdate(AmountTextBox, AmountPlaceholder);
        }

        private void AmountTextBox_Leave(object sender, EventArgs e)
        {
            UILogics.LeaveUpdate(AmountTextBox, AmountPlaceholder);
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UsernameTextBox.Text) || UsernameTextBox.Text == UsernamePlaceholder)
            {
                MessageBox.Show("Please enter a customer username.");
                return;
            }
            try
            {
                targetCustomer = new CustomerModel { Username = UsernameTextBox.Text };
                DataTable data = new DataReader().GetSingleData(targetCustomer, customer: true, employee: false);
                _FullnameLbl.Text = data.Rows[0][1].ToString();
                _eamilLbl.Text = data.Rows[0][2].ToString();
                _phoneLbl.Text = data.Rows[0][3].ToString();
                _NIDLbl.Text = data.Rows[0][4].ToString();
                BalanceLbl.Text = $"Balance: {((decimal)(data.Rows[0][5])).ToString("N", UILogics.SetPrecision(2))} $";
                _addressLbl.Text = data.Rows[0][6].ToString();
                _joinDateLbl.Text = data.Rows[0][7].ToString();
                infoPanel.Visible = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Customer not found.");
                targetCustomer = null;
                infoPanel.Visible = false;
            }
        }

        private void DepositBtn_Click(object sender, EventArgs e)
        {
            if (targetCustomer == null)
            {
                MessageBox.Show("Please search for a customer first.");
                return;
            }
            if (!decimal.TryParse(AmountTextBox.Text, out LastDepositAmount) || LastDepositAmount <= 0)
            {
                MessageBox.Show("Please enter a valid deposit amount.");
                return;
            }
            try
            {
                DataTable data = new DataReader().GetSingleData(targetCustomer, customer: true, employee: false);
                decimal currentBalance = (decimal)data.Rows[0][5];
                new CustomerOperation().UpdateBalance(targetCustomer, currentBalance + LastDepositAmount);

                // Use environment-agnostic path instead of hardcoded Windows path D:\DepositCache\
                string cacheDir = Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                    "BankApp", "DepositCache");
                Directory.CreateDirectory(cacheDir);
                File.WriteAllText(
                    Path.Combine(cacheDir, "last.txt"),
                    LastDepositAmount.ToString());

                MessageBox.Show($"Deposit of {LastDepositAmount:N2} $ to {targetCustomer.Username} was successful.");
                AmountTextBox.Text = AmountPlaceholder;
            }
            catch (Exception)
            {
                MessageBox.Show("Deposit failed. Please try again.");
            }
        }
    }
}
