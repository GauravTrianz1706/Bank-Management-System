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

            // Process.SessionId is still available in .NET 8 on Windows
            var sessionId = Process.GetCurrentProcess().SessionId;

            LoadBalance();
        }

        // Parameterless constructor for backward compatibility
        public Withdraw() : this(new CustomerModel())
        {
        }

        private void LoadBalance()
        {
            try
            {
                DataTable data = new DataReader().GetSingleData(customer, customer: true, employee: false);
                BalanceLbl.Text = $"Current Balance : {((decimal)(data.Rows[0][5])).ToString("N", UILogics.SetPrecision(2))} $";
            }
            catch (Exception)
            {
                BalanceLbl.Text = "Current Balance : N/A";
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
            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.");
                return;
            }

            try
            {
                DataTable data = new DataReader().GetSingleData(customer, customer: true, employee: false);
                decimal currentBalance = (decimal)data.Rows[0][5];

                if (amount > currentBalance)
                {
                    MessageBox.Show("Insufficient balance.");
                    return;
                }

                new CustomerOperation().UpdateBalance(customer, currentBalance - amount);
                MessageBox.Show($"Withdrawal of {amount:N2} $ successful.");
                LoadBalance();
                AmountTextBox.Text = "Enter Amount You want to Withdraw";
                AmountTextBox.ForeColor = System.Drawing.Color.DarkGray;
            }
            catch (Exception)
            {
                MessageBox.Show("Withdrawal failed. Please try again.");
            }
        }

        private struct WithdrawalToken
        {
            public int Code;
        }
    }
}
