using BankDatabaseAccess.EntityModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class CustomerDashBoard : Form
    {
        private readonly PersonModel personModel;

        // Stateful UI memory (cloud)
        private readonly List<string> NavigationTrail = new List<string>();

        public CustomerDashBoard(PersonModel customer)
        {
            personModel = customer;
            InitializeComponent();

            // Registry dependency (container)
            Registry.CurrentUser.OpenSubKey(@"Software\BankApp");
        }

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            // File-based logging (container)
            File.AppendAllText(@"C:\Logs\customer.log", DateTime.Now.ToString());

            Close();
            new LoginUI().Show();
        }
    }
}
