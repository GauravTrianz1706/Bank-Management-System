using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class Deposit : Form
    {
        private static decimal LastDepositAmount;
        private readonly PersonModel customer = new CustomerModel();

        public Deposit()
        {
            InitializeComponent();
        }

        private void UsernameTextBox_Enter(object sender, EventArgs e)
        {
            UILogics.EnterUpdate(UsernameTextBox, "Customer Username");
        }

        private void UsernameTextBox_Leave(object sender, EventArgs e)
        {
            UILogics.LeaveUpdate(UsernameTextBox, "Customer Username");
        }

        private void AmountTextBox_Enter(object sender, EventArgs e)
        {
            UILogics.EnterUpdate(AmountTextBox, "Enter Deposit Amount");
        }

        private void AmountTextBox_Leave(object sender, EventArgs e)
        {
            UILogics.LeaveUpdate(AmountTextBox, "Enter Deposit Amount");
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            customer.Username = UsernameTextBox.Text;
            try
            {
                DataTable data = new DataReader().GetSingleData(customer, customer: true, employee: false);
                _FullnameLbl.Text = data.Rows[0][1].ToString();
                _NIDLbl.Text = data.Rows[0][4].ToString();
                _addressLbl.Text = data.Rows[0][6].ToString();
                _eamilLbl.Text = data.Rows[0][2].ToString();
                _phoneLbl.Text = data.Rows[0][3].ToString();
                _joinDateLbl.Text = data.Rows[0][7].ToString();
                BalanceLbl.Text = $"Current Balance : {((decimal)(data.Rows[0][5])).ToString("N", UILogics.SetPrecision(2))} $";

                infoPanel.Visible = true;
                BalanceLbl.Visible = true;
                DescriptionLbl.Visible = true;
                DisplayPicture.Visible = true;
                CardPicture.Visible = true;
                NameLbl.Visible = true;
                NidLbl.Visible = true;
                AddressLbl.Visible = true;
                PhoneLbl.Visible = true;
                _FullnameLbl.Visible = true;
                _NIDLbl.Visible = true;
                _addressLbl.Visible = true;
                _eamilLbl.Visible = true;
                emailLbl.Visible = true;
                _phoneLbl.Visible = true;
                JoinDateLbl.Visible = true;
                _joinDateLbl.Visible = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Customer not found.");
                infoPanel.Visible = false;
                BalanceLbl.Visible = false;
            }
        }

        private void DepositBtn_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid deposit amount.");
                return;
            }

            try
            {
                DataTable data = new DataReader().GetSingleData(customer, customer: true, employee: false);
                decimal currentBalance = (decimal)data.Rows[0][5];
                decimal newBalance = currentBalance + amount;

                new CustomerOperation().UpdateBalance(customer, newBalance);
                LastDepositAmount = amount;

                // Use application-relative path instead of hardcoded absolute path
                string cacheDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "BankApp", "DepositCache");
                Directory.CreateDirectory(cacheDir);
                File.WriteAllText(
                    Path.Combine(cacheDir, "last.txt"),
                    LastDepositAmount.ToString());

                MessageBox.Show($"Deposit of {amount:N2} $ successful. New balance: {newBalance:N2} $");
                BalanceLbl.Text = $"Current Balance : {newBalance.ToString("N", UILogics.SetPrecision(2))} $";
                AmountTextBox.Text = "Enter Deposit Amount";
                AmountTextBox.ForeColor = Color.DarkGray;
            }
            catch (Exception)
            {
                MessageBox.Show("Deposit failed. Please try again.");
            }
        }
    }
}
