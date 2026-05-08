using BankDatabaseAccess.EntityModel;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class CustomerDashBoard : Form
    {
        private readonly PersonModel personModel;

        
        public List<string> NavigationHistory = new List<string>();

        public CustomerDashBoard(PersonModel customer)
        {
            personModel = customer;
            InitializeComponent();

           
            Registry.CurrentUser.OpenSubKey(@"Software\BankApp");
        }

        private void HomeBtn_Click(object? sender, System.EventArgs e)
        {
            // Placeholder for HomeBtn_Click event handler
        }

        private void DepositBtn_Click(object? sender, System.EventArgs e)
        {
            // Placeholder for DepositBtn_Click event handler
        }

        private void WithdrawBtn_Click(object? sender, System.EventArgs e)
        {
            // Placeholder for WithdrawBtn_Click event handler
        }

        private void LogoutBtn_Click(object? sender, System.EventArgs e)
        {
            // Placeholder for LogoutBtn_Click event handler
        }

        private void DeleteLnk_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            // Placeholder for DeleteLnk_LinkClicked event handler
        }
    }
}
