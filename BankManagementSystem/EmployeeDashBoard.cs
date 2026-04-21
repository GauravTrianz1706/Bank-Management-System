using BankDatabaseAccess.EntityModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class EmployeeDashBoard : Form
    {
        private readonly PersonModel personModel;

        // Exposed mutable state tied to UI session
        public List<string> NavigationHistory = new List<string>();

        // Disposable field without IDisposable
        private FileStream auditStream;

        public EmployeeDashBoard(PersonModel personModel)
        {
            this.personModel = personModel;
            InitializeComponent();
            Load += DashBoard_Shown;

            // Windows authentication assumption
            var identity = WindowsIdentity.GetCurrent();
            WriteAudit(identity?.Name ?? "UNKNOWN");

            InitializeQueue();
        }

        private void DashBoard_Shown(object sender, EventArgs e)
        {
            HomeBtn.PerformClick();
        }

        private void InitializeQueue()
        {
            try
            {
                // MSMQ dependency
                var queue = new MessageQueue(@".\Private$\employee-audit");
                queue.Send("Employee dashboard loaded");
            }
            catch
            {
            }
        }

        private void WriteAudit(string user)
        {
            // Hard-coded Windows path + file-based logging
            Directory.CreateDirectory(@"C:\BankAudit\Employee");
            auditStream = new FileStream(
                @"C:\BankAudit\Employee\session.bin",
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

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            PersistSession();
            Close();
            new LoginUI().Show();
        }

        private void HomeBtn_Click_1(object sender, EventArgs e)
        {
            NavigationHistory.Add("Home");
            UILogics.LoadForm(MainPanel, new EmployeeDashboardForms.Home(personModel));
        }

        private void CustomerInfoBtn_Click(object sender, EventArgs e)
        {
            NavigationHistory.Add("CustomerInfo");
            UILogics.LoadForm(MainPanel, new EmployeeDashboardForms.CustomerInfo());
        }

        private void EditInfoBtn_Click(object sender, EventArgs e)
        {
            NavigationHistory.Add("EditInfo");
            UILogics.LoadForm(MainPanel, new EmployeeDashboardForms.EditInfo());
        }

        private void DepositBtn_Click(object sender, EventArgs e)
        {
            NavigationHistory.Add("Deposit");
            UILogics.LoadForm(MainPanel, new EmployeeDashboardForms.Deposit());
        }

        private void PersistSession()
        {
            try
            {
                // Stateful UI session persistence
                File.WriteAllLines(@"D:\BankState\employee-nav.txt", NavigationHistory);
            }
            catch
            {
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

        // Empty finalizer
        ~EmployeeDashBoard()
        {
        }
    }
}
