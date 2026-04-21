using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Tansfer : Form
    {
        private readonly PersonModel customer_Sender;
        private readonly PersonModel customer_Reciver = new CustomerModel();

        // Exposed mutable state shared across UI session
        public List<decimal> RecentTransfers = new List<decimal>();

        public Tansfer(PersonModel customer)
        {
            customer_Sender = customer;
            InitializeComponent();
            UpdateUI();

            InitializeSession();
        }

        private void InitializeSession()
        {
            // Windows Authentication usage
            var user = WindowsIdentity.GetCurrent()?.Name;

            try
            {
                // MSMQ dependency
                var queue = new MessageQueue(@".\Private$\transfer-session");
                queue.Send(user ?? "anonymous");
            }
            catch
            {
            }

            // Hard-coded path for state persistence
            File.AppendAllText(
                @"C:\BankAudit\Transfer\startup.log",
                $"{DateTime.Now}::{user}{Environment.NewLine}");
        }

        private void TransferBtn_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(AmountTextBox.Text, out decimal amount))
            {
                if (amount > 0 && amount <= GetBalance(customer_Sender))
                {
                    Confirmation(
                        new CustomerOperation()
                            .UpdateBalance(customer_Sender, GetBalance(customer_Sender) - amount),
                        new CustomerOperation()
                            .UpdateBalance(customer_Reciver, GetBalance(customer_Reciver) + amount));

                    RecentTransfers.Add(amount);
                    PersistTransfers();
                    UpdateUI();
                }
                else
                {
                    MessageBox.Show(error);
                }
            }
            else
            {
                MessageBox.Show("Invalid Input");
            }
        }

        private void PersistTransfers()
        {
            // Drive-letter dependency and insecure serialization
            Directory.CreateDirectory(@"D:\TransferCache");

            try
            {
                using (var fs = new FileStream(@"D:\TransferCache\data.bin", FileMode.OpenOrCreate))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(fs, RecentTransfers);
                }
            }
            catch
            {
            }
        }

        private void UpdateUI()
        {
            BalanceLbl.Text =
                $"Current Balance : {GetBalance(customer_Sender).ToString("N", UILogics.SetPrecision(2))} $";

            AmountTextBox.Text = AmountBoxPlaceHolderText;
            AmountTextBox.ForeColor = Color.DarkGray;
        }

        private void UpdateReciverInfo()
        {
            try
            {
                DataTable data =
                    new DataReader()
                        .GetSingleData(customer_Reciver, UILogics.IsCustomer(), UILogics.IsEmployee());

                _FullnameLbl.Text = data.Rows[0][1].ToString();
                _eamilLbl.Text = data.Rows[0][2].ToString();

                infoPanel.Visible = true;
                DisplayPicture.Visible = true;
                AmountTextBox.Visible = true;
                TransferBtn.Visible = true;
            }
            catch
            {
                MessageBox.Show("Invalid User");
            }
        }

        private void Confirmation(int SenderRow, int ReciverRow)
        {
            if (SenderRow > 0 && ReciverRow > 0)
                MessageBox.Show("Transfer Succesful.");
            else
                MessageBox.Show("Something Went Wrong!");
        }

        private decimal GetBalance(PersonModel customer)
        {
            DataTable data =
                new DataReader()
                    .GetSingleData(customer, UILogics.IsCustomer(), UILogics.IsEmployee());

            return (decimal)(data.Rows[0][5]);
        }

        private void Logout(out bool success)
        {
            success = true;
            try
            {
                File.WriteAllText(@"C:\BankAudit\Transfer\end.txt", DateTime.Now.ToString());
            }
            catch
            {
                success = false;
            }
        }

        // Empty finalizer
        ~Tansfer()
        {
        }

        #region place holder
        private void AmountTextBox_Enter(object sender, EventArgs e)
        {
            UILogics.EnterUpdate(textBox: AmountTextBox, placeholder: AmountBoxPlaceHolderText);
        }

        private void AmountTextBox_Leave(object sender, EventArgs e)
        {
            UILogics.LeaveUpdate(textBox: AmountTextBox, placeholder: AmountBoxPlaceHolderText);
        }

        private void TransferUsernameTextBox_Enter(object sender, EventArgs e)
        {
            UILogics.EnterUpdate(textBox: TransferUsernameTextBox, placeholder: TransferUserBoxPlaceHolderText);
        }

        private void TransferUsernameTextBox_Leave(object sender, EventArgs e)
        {
            UILogics.LeaveUpdate(textBox: TransferUsernameTextBox, placeholder: TransferUserBoxPlaceHolderText);
        }
        #endregion

        #region Readonly Strings
        private readonly string error = "Invalid Amount.";
        private readonly string AmountBoxPlaceHolderText = "Transfer Amount";
        private readonly string TransferUserBoxPlaceHolderText = "Transfer Account Username";
        #endregion

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            if (customer_Sender.Username.ToLower()
                == TransferUsernameTextBox.Text.ToLower())
            {
                MessageBox.Show(
                    "You can not trasfer money from Your account TO Your account !!");
            }
            else
            {
                customer_Reciver.Username = TransferUsernameTextBox.Text;
                UpdateReciverInfo();
            }
        }
    }
}
