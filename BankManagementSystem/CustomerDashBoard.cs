using Azure.Data.AppConfiguration;
using BankDatabaseAccess.EntityModel;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class CustomerDashBoard : Form
    {
        private readonly PersonModel personModel;

        // Replaced static singleton List<string> with instance-scoped field to avoid
        // state inconsistency in multi-instance cloud deployments.
        // Each form instance maintains its own navigation history, preventing
        // cross-instance state contamination in cloud environments.
        private readonly List<string> navigationHistory;

        public CustomerDashBoard(PersonModel customer)
        {
            personModel = customer;
            InitializeComponent();

            // Initialize instance-scoped navigation history (stateless pattern).
            navigationHistory = new List<string>();

            // Replaced Microsoft.Win32.Registry access with Azure App Configuration.
            // Windows Registry doesn't exist on Linux cloud platforms.
            // Azure App Configuration provides cloud-native key-value configuration
            // with Workload Identity for credential-free access.
            string appConfigEndpoint = Environment.GetEnvironmentVariable("AZURE_APP_CONFIG_ENDPOINT");
            if (!string.IsNullOrEmpty(appConfigEndpoint))
            {
                var configClient = new ConfigurationClient(
                    new Uri(appConfigEndpoint),
                    new Azure.Identity.DefaultAzureCredential());

                // Read BankApp configuration from Azure App Configuration
                // (replaces Registry.CurrentUser.OpenSubKey(@"Software\BankApp")).
                try
                {
                    var setting = configClient.GetConfigurationSetting("BankApp:Settings");
                }
                catch (Exception)
                {
                    // Configuration key may not exist; continue with defaults.
                }
            }
        }
    }
}
