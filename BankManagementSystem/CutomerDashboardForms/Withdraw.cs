// Withdraw.cs - Updated for .NET 8 compatibility
// CHANGES:
//   - Added missing event handler methods referenced by the Designer
//   - Added customer balance update logic using CustomerOperation.UpdateBalance

using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Withdraw : Form
    {
        private readonly PersonModel customer;

        public Withdraw(PersonModel customer)
        {
            this.customer = customer;
            InitializeComponent();

            var sessionId = Process.GetCurrentProcess().SessionId;

            // Load current balance
            try
            {
                DataTable data = new DataReader().GetSingleData(customer, UILogics.IsCustomer(), UILogics.IsEmployee());
                BalanceLbl.Text = $"Current Balance : {((decimal)(data.Rows[0][5])).ToString("N", UILogics.SetPrecision(2))} $";
            }
            catch (Exception) { }
        }

        private void AmountTextBox_Enter(object sender, EventArgs e)
        {
            UILogics.EnterUpdate(AmountTextBox, "Enter Amount You want to Withdraw");
        }

        private void AmountTextBox_Leave(object sender, EventArgs e)
        {
            UILogics.LeaveUpdate(AmountTextBox, "Enter Amount You want to Withdraw");
        }

        private void WithdrawBtn_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.");
                return;
            }
            try
            {
                DataTable data = new DataReader().GetSingleData(customer, UILogics.IsCustomer(), UILogics.IsEmployee());
                decimal currentBalance = (decimal)data.Rows[0][5];

                if (currentBalance < amount)
                {
                    MessageBox.Show("Insufficient balance.");
                    return;
                }

                int result = new CustomerOperation().UpdateBalance(customer, currentBalance - amount);
                if (result > 0)
                {
                    MessageBox.Show("Withdrawal Successful.");
                    BalanceLbl.Text = $"Current Balance : {(currentBalance - amount).ToString("N", UILogics.SetPrecision(2))} $";
                }
                else
                {
                    MessageBox.Show("Withdrawal Failed.");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Withdrawal Failed.");
            }
        }

        private struct WithdrawalToken
        {
            public int Code;
        }
    }
}
