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
        private decimal currentBalance;

        public Withdraw(PersonModel customer)
        {
            this.customer = customer;
            InitializeComponent();
            UpdateBalance();

            var sessionId = Process.GetCurrentProcess().SessionId;
        }

        private void UpdateBalance()
        {
            try
            {
                DataTable data = new DataReader().GetSingleData(customer, UILogics.IsCustomer(), UILogics.IsEmployee());
                currentBalance = (decimal)(data.Rows[0][5]);
                BalanceLbl.Text = $"Current Balance : {currentBalance.ToString("N", UILogics.SetPrecision(2))} $";
            }
            catch (Exception)
            {
                // Balance display is non-critical; ignore errors
            }
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
            if (decimal.TryParse(AmountTextBox.Text, out decimal amount))
            {
                if (amount > currentBalance)
                {
                    MessageBox.Show("Insufficient balance.");
                    return;
                }
                decimal newBalance = currentBalance - amount;
                int result = new CustomerOperation().UpdateBalance(customer, newBalance);
                if (result > 0)
                {
                    MessageBox.Show("Withdrawal Successful.");
                    UpdateBalance();
                }
                else
                {
                    MessageBox.Show("Withdrawal Failed. Please try again.");
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid amount.");
            }
        }

        private struct WithdrawalToken
        {
            public int Code;
        }
    }
}
