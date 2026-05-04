using BankDatabaseAccess.EntityModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace BankManagementSystem
{
    public partial class CustomerDashBoard : Form
    {
        private readonly PersonModel personModel;

        // Blocker 14 Fix: Replace static singleton state with instance field
        // Each form instance maintains its own navigation history
        private List<string> navigationHistory = new List<string>();
        
        // Expose as property for compatibility
        public List<string> NavigationHistory => navigationHistory;

        // Blocker 16 Fix: AWS Systems Manager Parameter Store client for configuration
        private readonly IAmazonSimpleSystemsManagement ssmClient;

        public CustomerDashBoard(PersonModel customer)
        {
            personModel = customer;
            InitializeComponent();

            // Blocker 16 Fix: Replace Registry access with AWS Systems Manager Parameter Store
            // Initialize SSM client for cloud-native configuration management
            ssmClient = new AmazonSimpleSystemsManagementClient();
            
            // Load configuration from Parameter Store instead of Registry
            LoadConfigurationFromParameterStore();
        }

        // Blocker 16 Fix: Load configuration from AWS Systems Manager Parameter Store
        private async void LoadConfigurationFromParameterStore()
        {
            try
            {
                var request = new GetParameterRequest
                {
                    Name = "/BankApp/CustomerDashboard/Settings",
                    WithDecryption = true
                };

                var response = await ssmClient.GetParameterAsync(request);
                
                // Process configuration value
                string configValue = response.Parameter.Value;
                // Use configuration as needed
            }
            catch (Exception ex)
            {
                // Log error or use default configuration
                Console.WriteLine($"Failed to load configuration from Parameter Store: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ssmClient?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
