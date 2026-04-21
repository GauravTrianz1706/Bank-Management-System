using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace BankManagementSystem.Dashboard_Forms
{
    public partial class Withdraw : Form
    {
        private readonly PersonModel customer;

        public Withdraw(PersonModel customer)
        {
            this.customer = customer;
            InitializeComponent();

            // Windows-only environment assumption (container)
            Process.GetCurrentProcess().SessionId.ToString();
        }
    }
}
