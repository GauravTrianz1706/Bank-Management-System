using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using BankManagementSystem.Dashboard_Forms;
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
            UILogics.LoadForm(MainPanel, new Home(customer));
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
            if (UILogics.DeleteWarning(personModel))
            {
                this.Close();
            }
        }
    }
}
