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

            // CONTAINERIZATION FIX: Replaced Windows Registry access (Registry.CurrentUser.OpenSubKey)
            // with environment variable for cross-platform compatibility
            // Blocker IDs: blocker-4, blocker-5 (cz-dotnet-0002 - Registry Access)
            // In production, use Azure App Configuration for centralized, cloud-native configuration
            // with feature flags, key-value pairs, and dynamic updates without pod restarts
            string appConfig = Environment.GetEnvironmentVariable("BANK_APP_CONFIG") 
                ?? "default-config";
        }
    }
}
