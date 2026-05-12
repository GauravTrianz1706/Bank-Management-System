using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Tansfer : Form
    {
        // System.Messaging.MessageQueue is not available in .NET 8.
        // Replaced with a simple in-memory audit log list.
        private readonly System.Collections.Generic.List<string> _auditLog = new System.Collections.Generic.List<string>();

        private int[] recentTransfers = new[] { 100, 200 };
        public int[] RecentTransfers => recentTransfers;

        private readonly PersonModel customer;

        public Tansfer(PersonModel customer)
        {
            this.customer = customer;
            InitializeComponent();
            _auditLog.Add($"Transfer initiated at {DateTime.Now}");
            LoadBalance();
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
            UILogics.EnterUpdate(AmountTextBox, "Transfer Amount");
        }

        private void AmountTextBox_Leave(object sender, EventArgs e)
        {
            UILogics.LeaveUpdate(AmountTextBox, "Transfer Amount");
        }

        private void TransferUsernameTextBox_Enter(object sender, EventArgs e)
        {
            UILogics.EnterUpdate(TransferUsernameTextBox, "Transfer Account Username");
        }

        private void TransferUsernameTextBox_Leave(object sender, EventArgs e)
        {
            UILogics.LeaveUpdate(TransferUsernameTextBox, "Transfer Account Username");
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            try
            {
                PersonModel target = new CustomerModel { Username = TransferUsernameTextBox.Text };
                DataTable data = new DataReader().GetSingleData(target, customer: true, employee: false);
                _FullnameLbl.Text = data.Rows[0][1].ToString();
                _eamilLbl.Text = data.Rows[0][2].ToString();
                infoPanel.Visible = true;
                AmountTextBox.Visible = true;
                TransferBtn.Visible = true;
                _auditLog.Add($"Searched user: {TransferUsernameTextBox.Text} at {DateTime.Now}");
            }
            catch (Exception)
            {
                MessageBox.Show("User not found.");
                infoPanel.Visible = false;
                AmountTextBox.Visible = false;
                TransferBtn.Visible = false;
            }
        }

        private void TransferBtn_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.");
                return;
            }

            try
            {
                // Get sender's current balance
                DataTable senderData = new DataReader().GetSingleData(customer, customer: true, employee: false);
                decimal senderBalance = (decimal)senderData.Rows[0][5];

                if (amount > senderBalance)
                {
                    MessageBox.Show("Insufficient balance.");
                    return;
                }

                // Get receiver's current balance
                PersonModel receiver = new CustomerModel { Username = TransferUsernameTextBox.Text };
                DataTable receiverData = new DataReader().GetSingleData(receiver, customer: true, employee: false);
                decimal receiverBalance = (decimal)receiverData.Rows[0][5];

                // Update balances
                new CustomerOperation().UpdateBalance(customer, senderBalance - amount);
                new CustomerOperation().UpdateBalance(receiver, receiverBalance + amount);

                _auditLog.Add($"Transferred {amount} to {TransferUsernameTextBox.Text} at {DateTime.Now}");
                MessageBox.Show($"Transfer of {amount:N2} $ successful.");
                LoadBalance();
                infoPanel.Visible = false;
                AmountTextBox.Visible = false;
                TransferBtn.Visible = false;
                TransferUsernameTextBox.Text = "Transfer Account Username";
                TransferUsernameTextBox.ForeColor = Color.DarkGray;
            }
            catch (Exception)
            {
                MessageBox.Show("Transfer failed. Please try again.");
            }
        }
    }
}
