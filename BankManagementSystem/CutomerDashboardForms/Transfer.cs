// Transfer.cs - Updated for .NET 8 compatibility
// CHANGES:
//   - Removed System.Messaging.MessageQueue usage entirely.
//     System.Messaging (MSMQ) is NOT available in .NET 8. The namespace was
//     removed from .NET Core/.NET 5+. Replaced with System.Diagnostics.Trace
//     as a lightweight alternative for audit logging.
//     For production use, replace with a proper message broker
//     (e.g., RabbitMQ, Azure Service Bus, or Microsoft.Extensions.Logging).
//   - Added missing event handler methods referenced by the Designer

using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Data;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Tansfer : Form
    {
        // REMOVED: System.Messaging.MessageQueue field - not available in .NET 8.
        // REPLACEMENT: Use Trace or a proper logging/messaging framework.

        private int[] recentTransfers = new[] { 100, 200 };
        public int[] RecentTransfers => recentTransfers;

        private readonly PersonModel customer;

        public Tansfer(PersonModel customer)
        {
            this.customer = customer;
            InitializeComponent();

            // CHANGED: MessageQueue.Send replaced with Trace for .NET 8 compatibility.
            // Replace with a proper messaging/logging solution for production use.
            System.Diagnostics.Trace.TraceInformation("Transfer initiated");

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
                DisplayPicture.Visible = true;
                AmountTextBox.Visible = true;
                TransferBtn.Visible = true;
            }
            catch (Exception)
            {
                MessageBox.Show("User not found.");
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
                DataTable senderData = new DataReader().GetSingleData(customer, UILogics.IsCustomer(), UILogics.IsEmployee());
                decimal senderBalance = (decimal)senderData.Rows[0][5];

                if (senderBalance < amount)
                {
                    MessageBox.Show("Insufficient balance.");
                    return;
                }

                // Get receiver's current balance
                PersonModel target = new CustomerModel { Username = TransferUsernameTextBox.Text };
                DataTable receiverData = new DataReader().GetSingleData(target, customer: true, employee: false);
                decimal receiverBalance = (decimal)receiverData.Rows[0][5];

                // Deduct from sender
                new CustomerOperation().UpdateBalance(customer, senderBalance - amount);
                // Add to receiver
                new CustomerOperation().UpdateBalance(target, receiverBalance + amount);

                MessageBox.Show("Transfer Successful.");
                BalanceLbl.Text = $"Current Balance : {(senderBalance - amount).ToString("N", UILogics.SetPrecision(2))} $";
            }
            catch (Exception)
            {
                MessageBox.Show("Transfer Failed.");
            }
        }
    }
}
