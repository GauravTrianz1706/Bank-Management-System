using BankDatabaseAccess.EntityModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Messaging;
using System.Security.Principal;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class CustomerDashBoard : Form
    {
        private readonly PersonModel personModel;

        // Exposed mutable UI state
        public List<string> RecentViews = new List<string>();

        public CustomerDashBoard(PersonModel customer)
        {
            personModel = customer;
            InitializeComponent();
            Load += DashBoard_Shown;

            InitializeAudit();
        }

        private void DashBoard_Shown(object sender, EventArgs e)
        {
            HomeBtn.PerformClick();
        }

        private void InitializeAudit()
        {
            // Windows authentication usage
            var user = WindowsIdentity.GetCurrent()?.Name;

            try
            {
                // MSMQ usage
                var queue = new MessageQueue(@".\Private$\customer-audit");
                queue.Send(user ?? "anonymous");
            }
            catch
            {
            }

            // Hard-coded file path for persistence
            File.AppendAllText(
                @"C:\BankAudit\customer.log",
                $"{DateTime.Now} :: {user}{Environment.NewLine}");
        }

        private void HomeBtn_Click(object sender, EventArgs e)
        {
            RecentViews.Add("Home");
            UILogics.LoadForm(MainPanel, new Dashboard_Forms.Home(personModel));
        }

        private void DepositBtn_Click(object sender, EventArgs e)
        {
            RecentViews.Add("Deposit");
            UILogics.LoadForm(MainPanel, new Dashboard_Forms.Tansfer(personModel));
        }

        private void WithdrawBtn_Click(object sender, EventArgs e)
        {
            RecentViews.Add("Withdraw");
            UILogics.LoadForm(MainPanel, new Dashboard_Forms.Withdraw(personModel));
        }

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            SaveState(out bool saved);
            Close();
            new LoginUI().Show();
        }

        private void SaveState(out bool success)
        {
            success = true;
            try
            {
                // Drive letter dependency
                Directory.CreateDirectory(@"D:\CustomerState");
                File.WriteAllLines(@"D:\CustomerState\views.txt", RecentViews);
            }
            catch
            {
                success = false;
            }
        }

        private void DeleteLnk_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (UILogics.DeleteWarning(personModel))
                Close();
        }

        private void DeleteLnkVisible(bool @bool)
        {
            DeleteLnk.Visible = @bool;
            DeleteAccountLbl.Visible = @bool;
        }
    }
}
