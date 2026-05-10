using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Data;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Withdraw : Form
    {
        private PersonModel? customer;

        private const string AmountPlaceholder = "Enter Amount You want to Withdraw";

        private struct WithdrawalToken
        {
            public int Code;
        }

        public Withdraw()
        {
            InitializeComponent();
        }

        public Withdraw(PersonModel customer) : this()
        {
            this.customer = customer;
            UpdateBalance();
        }

        private void UpdateBalance()
        {
            if (customer == null) return;
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
            UILogics.EnterUpdate(AmountTextBox, AmountPlaceholder);
        }

        private void AmountTextBox_Leave(object sender, EventArgs e)
        {
            UILogics.LeaveUpdate(AmountTextBox, AmountPlaceholder);
        }

        private void WithdrawBtn_Click(object sender, EventArgs e)
        {
            if (customer == null)
            {
                MessageBox.Show("No customer session found.");
                return;
            }
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
                MessageBox.Show($"Withdrawal of {amount:N2} $ was successful.");
                UpdateBalance();
                AmountTextBox.Text = AmountPlaceholder;
            }
            catch (Exception)
            {
                MessageBox.Show("Withdrawal failed. Please try again.");
            }
        }
    }
}
