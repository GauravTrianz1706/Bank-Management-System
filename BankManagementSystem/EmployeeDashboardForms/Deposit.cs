// Deposit.cs - Updated for .NET 8 compatibility
// CHANGES:
//   - Replaced hardcoded absolute path (D:\DepositCache\last.txt) with a path
//     relative to AppContext.BaseDirectory per .NET 8 best practices.
//     Hardcoded drive-letter paths are a breaking issue on non-Windows or
//     containerized .NET 8 deployments.
//   - Added missing event handler methods referenced by the Designer

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

        private PersonModel? currentCustomer;

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
            try
            {
                currentCustomer = new CustomerModel { Username = UsernameTextBox.Text };
                DataTable data = new DataReader().GetSingleData(currentCustomer, customer: true, employee: false);
                _FullnameLbl.Text = data.Rows[0][1].ToString();
                _NIDLbl.Text = data.Rows[0][4].ToString();
                _phoneLbl.Text = data.Rows[0][3].ToString();
                _addressLbl.Text = data.Rows[0][6].ToString();
                _eamilLbl.Text = data.Rows[0][2].ToString();
                _joinDateLbl.Text = data.Rows[0][7].ToString();
                BalanceLbl.Text = $"Current Balance : {((decimal)(data.Rows[0][5])).ToString("N", UILogics.SetPrecision(2))} $";
                infoPanel.Visible = true;
                DescriptionLbl.Visible = true;
                BalanceLbl.Visible = true;
                DisplayPicture.Visible = true;
                CardPicture.Visible = true;
                NameLbl.Visible = true;
                NidLbl.Visible = true;
                AddressLbl.Visible = true;
                PhoneLbl.Visible = true;
                JoinDateLbl.Visible = true;
                _joinDateLbl.Visible = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Customer not found.");
            }
        }

        private void DepositBtn_Click(object sender, System.EventArgs e)
        {
            decimal.TryParse(AmountTextBox.Text, out LastDepositAmount);

            if (currentCustomer == null || LastDepositAmount <= 0)
            {
                MessageBox.Show("Please search for a customer and enter a valid amount.");
                return;
            }

            try
            {
                DataTable data = new DataReader().GetSingleData(currentCustomer, customer: true, employee: false);
                decimal currentBalance = (decimal)data.Rows[0][5];
                int result = new CustomerOperation().UpdateBalance(currentCustomer, currentBalance + LastDepositAmount);
                if (result > 0)
                {
                    MessageBox.Show("Deposit Successful.");
                    BalanceLbl.Text = $"Current Balance : {(currentBalance + LastDepositAmount).ToString("N", UILogics.SetPrecision(2))} $";
                }
                else
                {
                    MessageBox.Show("Deposit Failed.");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Deposit Failed.");
            }

            // CHANGED: Hardcoded absolute path (D:\DepositCache\last.txt) replaced
            // with AppContext.BaseDirectory-relative path for .NET 8 compatibility.
            string cacheDir = Path.Combine(AppContext.BaseDirectory, "DepositCache");
            Directory.CreateDirectory(cacheDir);
            File.WriteAllText(
                Path.Combine(cacheDir, "last.txt"),
                LastDepositAmount.ToString());
        }
    }
}
