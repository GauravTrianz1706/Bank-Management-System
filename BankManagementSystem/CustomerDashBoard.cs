using BankDatabaseAccess.EntityModel;
using BankManagementSystem.Dashboard_Forms;
using System.Collections.Generic;
using System.Windows.Forms;

// NOTE: Microsoft.Win32.Registry is Windows-only and is available in .NET 8
// via the Microsoft.Win32.Registry NuGet package or the built-in Windows
// compatibility shim (net8.0-windows TFM). The Registry call has been removed
// here because it was only used for a side-effect read with no result consumed.

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
        }

        private void HomeBtn_Click(object sender, System.EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new Home(personModel));
        }

        private void DepositBtn_Click(object sender, System.EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new Tansfer(personModel));
        }

        private void WithdrawBtn_Click(object sender, System.EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new Withdraw(personModel));
        }

        private void LogoutBtn_Click(object sender, System.EventArgs e)
        {
            this.Close();
            new WelcomeUI().Show();
        }

        private void DeleteLnk_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UILogics.DeleteWarning(personModel);
        }
    }
}
