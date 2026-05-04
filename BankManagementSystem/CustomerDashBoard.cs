using BankDatabaseAccess.EntityModel;
using System;
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

            // Fixed: Replaced Windows Registry access with environment variable configuration
            // Configuration should be loaded from AWS Systems Manager Parameter Store or environment variables
            string appConfig = Environment.GetEnvironmentVariable("BANK_APP_CONFIG") ?? "default";
        }
    }
}
