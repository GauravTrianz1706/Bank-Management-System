using BankDatabaseAccess.EntityModel;
using Microsoft.Extensions.Configuration;
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
            // Registry values should be migrated to AWS Systems Manager Parameter Store
            // and accessed via environment variables
            string bankAppConfig = Environment.GetEnvironmentVariable("BANK_APP_CONFIG") ?? "default";
        }
    }
}
