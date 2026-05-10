using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Data;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Tansfer : Form
    {
        // System.Messaging.MessageQueue is not available in .NET 8.
        // Replaced with a simple in-memory log list for transfer audit tracking.
        private readonly System.Collections.Generic.List<string> transferAuditLog =
            new System.Collections.Generic.List<string>();

        private int[] recentTransfers = new[] { 100, 200 };
        public int[] RecentTransfers => recentTransfers;

        private readonly PersonModel customer;
        private PersonModel? transferTarget;

        private const string AmountPlaceholder = "Transfer Amount";
        private const string UsernamePlaceholder = "Transfer Account Username";

        public Tansfer(PersonModel customer)
        {
            this.customer = customer;
            InitializeComponent();
            // Log transfer initiation in-memory instead of MSMQ
            transferAuditLog.Add("Transfer initiated");
            UpdateBalance();
        }

        private void UpdateBalance()
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
            UILogics.EnterUpdate(AmountTextBox, AmountPlaceholder);
        }

        private void AmountTextBox_Leave(object sender, EventArgs e)
        {
            UILogics.LeaveUpdate(AmountTextBox, AmountPlaceholder);
        }

        private void TransferUsernameTextBox_Enter(object sender, EventArgs e)
        {
            UILogics.EnterUpdate(TransferUsernameTextBox, UsernamePlaceholder);
        }

        private void TransferUsernameTextBox_Leave(object sender, EventArgs e)
        {
            UILogics.LeaveUpdate(TransferUsernameTextBox, UsernamePlaceholder);
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TransferUsernameTextBox.Text) || TransferUsernameTextBox.Text == UsernamePlaceholder)
            {
                MessageBox.Show("Please enter a username to search.");
                return;
            }
            try
            {
                transferTarget = new CustomerModel { Username = TransferUsernameTextBox.Text };
                DataTable data = new DataReader().GetSingleData(transferTarget, customer: true, employee: false);
                _FullnameLbl.Text = data.Rows[0][1].ToString();
                _eamilLbl.Text = data.Rows[0][2].ToString();
                infoPanel.Visible = true;
                DisplayPicture.Visible = true;
                AmountTextBox.Visible = true;
                TransferBtn.Visible = true;
            }
            catch (Exception)
            {
                MessageBox.Show("User not found.");
                infoPanel.Visible = false;
                DisplayPicture.Visible = false;
                AmountTextBox.Visible = false;
                TransferBtn.Visible = false;
                transferTarget = null;
            }
        }

        private void TransferBtn_Click(object sender, EventArgs e)
        {
            if (transferTarget == null)
            {
                MessageBox.Show("Please search for a valid recipient first.");
                return;
            }
            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.");
                return;
            }
            try
            {
                DataTable senderData = new DataReader().GetSingleData(customer, customer: true, employee: false);
                decimal senderBalance = (decimal)senderData.Rows[0][5];
                if (amount > senderBalance)
                {
                    MessageBox.Show("Insufficient balance.");
                    return;
                }
                DataTable receiverData = new DataReader().GetSingleData(transferTarget, customer: true, employee: false);
                decimal receiverBalance = (decimal)receiverData.Rows[0][5];

                new CustomerOperation().UpdateBalance(customer, senderBalance - amount);
                new CustomerOperation().UpdateBalance(transferTarget, receiverBalance + amount);

                transferAuditLog.Add($"Transferred {amount} to {transferTarget.Username}");
                MessageBox.Show($"Transfer of {amount:N2} $ to {transferTarget.Username} was successful.");
                UpdateBalance();
                AmountTextBox.Text = AmountPlaceholder;
            }
            catch (Exception)
            {
                MessageBox.Show("Transfer failed. Please try again.");
            }
        }
    }
}
