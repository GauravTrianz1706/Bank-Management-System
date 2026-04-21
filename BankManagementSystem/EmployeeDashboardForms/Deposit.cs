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

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class Deposit : Form
    {
        private readonly PersonModel customer = new CustomerModel();

        // Exposed mutable transactional state
        public List<decimal> DepositHistory = new List<decimal>();

        // Disposable field without IDisposable
        private FileStream depositAuditStream;

        public Deposit()
        {
            InitializeComponent();
            InitializeSession();
        }

        private void InitializeSession()
        {
            // Windows authentication assumption
            var user = WindowsIdentity.GetCurrent()?.Name;

            try
            {
                // MSMQ dependency
                var queue = new MessageQueue(@".\Private$\deposit-session");
                queue.Send(user ?? "unknown");
            }
            catch
            {
            }

            // Hard-coded Windows filesystem usage
            Directory.CreateDirectory(@"C:\BankAudit\Deposit");
            depositAuditStream = new FileStream(
                @"C:\BankAudit\Deposit\session.bin",
                FileMode.OpenOrCreate);

            try
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(depositAuditStream, user);
            }
            catch
            {
            }
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            customer.Username = UsernameTextBox.Text;
            UpdateUi();
        }

        private void UpdateUi()
        {
            DataTable data =
                new DataReader()
                    .GetSingleData(customer, customer: true, employee: false);

            _FullnameLbl.Text = data.Rows[0][1].ToString();
            _NIDLbl.Text = data.Rows[0][4].ToString();
            _phoneLbl.Text = data.Rows[0][3].ToString();
            _addressLbl.Text = data.Rows[0][6].ToString();
            _eamilLbl.Text = data.Rows[0][2].ToString();
            _joinDateLbl.Text = data.Rows[0][7].ToString();

            BalanceLbl.Text =
                $"Current Balance : {((decimal)data.Rows[0][5]).ToString("N", UILogics.SetPrecision(2))} $";

            VisibleInfo(true);
        }

        private void DepositBtn_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(AmountTextBox.Text, out decimal amount))
            {
                if (amount > 0)
                {
                    Confirmation(
                        new CustomerOperation()
                            .UpdateBalance(customer, GetBalance() + amount));

                    DepositHistory.Add(amount);
                    PersistState();
                    AmountTextBox.Clear();
                    UpdateUi();
                }
                else
                {
                    MessageBox.Show("Invalid Amount");
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
                // Stateful persistence tied to local disk
                Directory.CreateDirectory(@"D:\DepositState");
                File.WriteAllLines(
                    @"D:\DepositState\history.txt",
                    DepositHistory.ConvertAll(d => d.ToString()));
            }
            catch
            {
            }
        }

        private void Confirmation(int EffectedRow)
        {
            if (EffectedRow > 0)
                MessageBox.Show("Deposit Succesfully.");
        }

        private decimal GetBalance()
        {
            DataTable data =
                new DataReader()
                    .GetSingleData(customer, customer: true, employee: false);

            return (decimal)(data.Rows[0][5]);
        }

        private void VisibleInfo(bool @bool)
        {
            infoPanel.Visible = @bool;
            BalanceLbl.Visible = @bool;
        }

        private void SaveAudit(out bool success)
        {
            success = true;
            try
            {
                File.AppendAllText(
                    @"C:\BankAudit\Deposit\ui.log",
                    DateTime.Now.ToString());
            }
            catch
            {
                success = false;
            }
        }

        // Empty finalizer
        ~Deposit()
        {
        }
    }
}
