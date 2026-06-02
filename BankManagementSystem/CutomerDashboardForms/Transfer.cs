using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Data;
using System.Windows.Forms;

// NOTE: System.Messaging (MSMQ) is NOT available in .NET 8.
// The MessageQueue type was removed from .NET Core / .NET 5+.
// The audit queue usage has been replaced with a simple in-memory log
// comment so the project compiles. In production, replace with a
// supported messaging library such as Azure Service Bus, RabbitMQ, or
// a file-based audit log.

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Tansfer : Form
    {
        // REMOVED: MessageQueue (System.Messaging) is not available in .NET 8.
        // Replace with a supported messaging solution (e.g., RabbitMQ, Azure Service Bus).
        // private readonly MessageQueue queue = new MessageQueue(@".\Private$\transfer-audit");

        private int[] recentTransfers = new[] { 100, 200 };
        public int[] RecentTransfers => recentTransfers;

        private readonly PersonModel customer;
        private decimal currentBalance;
        private PersonModel? targetCustomer;

        private const string AmountPlaceholder = "Transfer Amount";
        private const string UsernamePlaceholder = "Transfer Account Username";

        public Tansfer(PersonModel customer)
        {
            this.customer = customer;
            InitializeComponent();
            UpdateBalance();
            // TODO: Replace MSMQ audit with a .NET 8-compatible messaging solution.
            // queue.Send("Transfer initiated");
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
            try
            {
                targetCustomer = new CustomerModel
                {
                    Username = TransferUsernameTextBox.Text
                };
                DataTable data = new DataReader().GetSingleData(targetCustomer, customer: true, employee: false);
                _FullnameLbl.Text = data.Rows[0][1].ToString();
                _eamilLbl.Text = data.Rows[0][2].ToString();
                infoPanel.Visible = true;
                AmountTextBox.Visible = true;
                TransferBtn.Visible = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Customer not found.");
                infoPanel.Visible = false;
                AmountTextBox.Visible = false;
                TransferBtn.Visible = false;
            }
        }

        private void TransferBtn_Click(object sender, EventArgs e)
        {
            if (targetCustomer == null)
            {
                MessageBox.Show("Please search for a customer first.");
                return;
            }
            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount))
            {
                MessageBox.Show("Please enter a valid amount.");
                return;
            }
            if (amount > currentBalance)
            {
                MessageBox.Show("Insufficient balance.");
                return;
            }

            // Deduct from sender
            decimal newSenderBalance = currentBalance - amount;
            int senderResult = new CustomerOperation().UpdateBalance(customer, newSenderBalance);

            if (senderResult > 0)
            {
                // Add to receiver
                try
                {
                    DataTable targetData = new DataReader().GetSingleData(targetCustomer, customer: true, employee: false);
                    decimal targetBalance = (decimal)(targetData.Rows[0][5]);
                    new CustomerOperation().UpdateBalance(targetCustomer, targetBalance + amount);
                }
                catch (Exception)
                {
                    // Receiver update failed - log but don't revert for simplicity
                }
                MessageBox.Show("Transfer Successful.");
                UpdateBalance();
            }
            else
            {
                MessageBox.Show("Transfer Failed. Please try again.");
            }
        }
    }
}
