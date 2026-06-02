using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

// NOTE: Hard-coded absolute path (D:\DepositCache\last.txt) replaced with a
// path relative to the application base directory so it works on any machine.

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class Deposit : Form
    {
        private static decimal LastDepositAmount;
        private readonly PersonModel customer = new CustomerModel();

        private const string AmountPlaceholder = "Enter Deposit Amount";
        private const string UsernamePlaceholder = "Customer Username";

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
            customer.Username = UsernameTextBox.Text;
            FillInfo();
        }

        private void FillInfo()
        {
            try
            {
                DataTable data = new DataReader().GetSingleData(customer, customer: true, employee: false);
                _FullnameLbl.Text = data.Rows[0][1].ToString();
                _NIDLbl.Text = data.Rows[0][4].ToString();
                _addressLbl.Text = data.Rows[0][6].ToString();
                _eamilLbl.Text = data.Rows[0][2].ToString();
                _phoneLbl.Text = data.Rows[0][3].ToString();
                _joinDateLbl.Text = data.Rows[0][7].ToString();
                decimal balance = (decimal)(data.Rows[0][5]);
                BalanceLbl.Text = $"Current Balance : {balance.ToString("N", UILogics.SetPrecision(2))} $";

                // Show info panel and related labels
                infoPanel.Visible = true;
                BalanceLbl.Visible = true;
                DescriptionLbl.Visible = true;
                NameLbl.Visible = true;
                NidLbl.Visible = true;
                AddressLbl.Visible = true;
                PhoneLbl.Visible = true;
                emailLbl.Visible = true;
                JoinDateLbl.Visible = true;
                _FullnameLbl.Visible = true;
                _NIDLbl.Visible = true;
                _addressLbl.Visible = true;
                _eamilLbl.Visible = true;
                _phoneLbl.Visible = true;
                _joinDateLbl.Visible = true;
                DisplayPicture.Visible = true;
                CardPicture.Visible = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Customer not found.");
            }
        }

        private void DepositBtn_Click(object sender, EventArgs e)
        {
            decimal.TryParse(AmountTextBox.Text, out LastDepositAmount);

            if (LastDepositAmount <= 0)
            {
                MessageBox.Show("Please enter a valid deposit amount.");
                return;
            }

            try
            {
                DataTable data = new DataReader().GetSingleData(customer, customer: true, employee: false);
                decimal currentBalance = (decimal)(data.Rows[0][5]);
                decimal newBalance = currentBalance + LastDepositAmount;
                int result = new CustomerOperation().UpdateBalance(customer, newBalance);
                if (result > 0)
                {
                    MessageBox.Show("Deposit Successful.");
                    FillInfo();
                }
                else
                {
                    MessageBox.Show("Deposit Failed. Please try again.");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Customer not found. Please search first.");
            }

            // Replaced hard-coded absolute path (D:\DepositCache\last.txt) with
            // a path relative to the application base directory.
            string cacheDir = Path.Combine(AppContext.BaseDirectory, "DepositCache");
            Directory.CreateDirectory(cacheDir);
            File.WriteAllText(
                Path.Combine(cacheDir, "last.txt"),
                LastDepositAmount.ToString());
        }
    }
}
