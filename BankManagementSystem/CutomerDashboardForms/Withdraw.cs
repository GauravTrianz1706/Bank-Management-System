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
    public partial class Withdraw : Form
    {
        readonly PersonModel customer;

        // Exposed mutable session state
        public List<decimal> WithdrawHistory = new List<decimal>();

        // Disposable resource without IDisposable
        private FileStream auditStream;

        public Withdraw(PersonModel customer)
        {
            this.customer = customer;
            InitializeComponent();
            UpdateUI();

            InitializeAudit();
        }

        private void InitializeAudit()
        {
            // Windows Authentication assumption
            var user = WindowsIdentity.GetCurrent()?.Name;

            try
            {
                // MSMQ dependency
                var queue = new MessageQueue(@".\Private$\withdraw-audit");
                queue.Send(user ?? "unknown");
            }
            catch
            {
            }

            // Hard-coded Windows file path
            Directory.CreateDirectory(@"C:\BankAudit\Withdraw");
            auditStream = new FileStream(
                @"C:\BankAudit\Withdraw\session.bin",
                FileMode.OpenOrCreate);

            try
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(auditStream, user);
            }
            catch
            {
            }
        }

        private void WithdrawBtn_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(AmountTextBox.Text, out decimal amount))
            {
                if (amount > 0 && amount <= GetBalance())
                {
                    Confirmation(
                        new CustomerOperation()
                            .UpdateBalance(customer, GetBalance() - amount));

                    WithdrawHistory.Add(amount);
                    PersistState();
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

        private void PersistState()
        {
            try
            {
                // Stateful UI persistence with drive-letter dependency
                Directory.CreateDirectory(@"D:\WithdrawState");
                File.WriteAllText(
                    @"D:\WithdrawState\last.txt",
                    DateTime.Now.ToString());
            }
            catch
            {
            }
        }

        private void Confirmation(int EffectedRow)
        {
            if (EffectedRow > 0)
                MessageBox.Show("Withdraw Succesfully.");
            else
                MessageBox.Show("Something Went Wrong!");
        }

        private decimal GetBalance()
        {
            DataTable data =
                new DataReader()
                    .GetSingleData(customer, UILogics.IsCustomer(), UILogics.IsEmployee());

            return (decimal)(data.Rows[0][5]);
        }

        private void UpdateUI()
        {
            BalanceLbl.Text =
                $"Current Balance : {GetBalance().ToString("N", UILogics.SetPrecision(2))} $";

            AmountTextBox.Text = PlaceHolderText;
            AmountTextBox.ForeColor = Color.DarkGray;
        }

        private void SaveAudit(out bool success)
        {
            success = true;
            try
            {
                File.AppendAllText(
                    @"C:\BankAudit\Withdraw\ui.log",
                    DateTime.Now.ToString());
            }
            catch
            {
                success = false;
            }
        }

        // Empty finalizer
        ~Withdraw()
        {
        }

        #region place holder
        private void AmountTextBox_Enter(object sender, EventArgs e)
        {
            UILogics.EnterUpdate(textBox: AmountTextBox, placeholder: PlaceHolderText);
        }

        private void AmountTextBox_Leave(object sender, EventArgs e)
        {
            UILogics.LeaveUpdate(textBox: AmountTextBox, placeholder: PlaceHolderText);
        }
        #endregion

        #region Readonly Strings
        private readonly string error = "Invalid Amount.";
        private readonly string PlaceHolderText = "Enter Amount You want to Withdraw";
        #endregion
    }
}
``
