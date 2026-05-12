using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using BankManagementSystem.Dashboard_Forms;
using System;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class CustomerDashBoard : Form
    {
        private readonly PersonModel personModel;

        public System.Collections.Generic.List<string> NavigationHistory = new System.Collections.Generic.List<string>();

        public CustomerDashBoard(PersonModel customer)
        {
            personModel = customer;
            InitializeComponent();
            HomeBtn_Click(this, EventArgs.Empty);
        }

        private void HomeBtn_Click(object sender, EventArgs e)
        {
            NavigationHistory.Add("Home");
            UILogics.LoadForm(MainPanel, new Home(personModel));
        }

        private void DepositBtn_Click(object sender, EventArgs e)
        {
            NavigationHistory.Add("Transfer");
            UILogics.LoadForm(MainPanel, new Tansfer(personModel));
        }

        private void WithdrawBtn_Click(object sender, EventArgs e)
        {
            NavigationHistory.Add("Withdraw");
            UILogics.LoadForm(MainPanel, new Withdraw(personModel));
        }

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            this.Close();
            new WelcomeUI().Show();
        }

        private void DeleteLnk_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (UILogics.DeleteWarning(personModel))
            {
                this.Close();
            }
        }
    }
}
