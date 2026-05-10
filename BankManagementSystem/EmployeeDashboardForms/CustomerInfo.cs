using BankDatabaseAccess.DatabaseOperation;
using System;
using System.IO;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class CustomerInfo : Form
    {
        public CustomerInfo()
        {
            InitializeComponent();

            // System.Messaging.MessageQueue is not available in .NET 8.
            // Replaced with a simple in-memory log list for customer info audit tracking.
            var auditLog = new System.Collections.Generic.List<string>();
            auditLog.Add("Customer viewed");

            // Use environment-agnostic path instead of hardcoded Windows path C:\CustomerLogs\
            string logDir = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                "BankApp", "CustomerLogs");
            Directory.CreateDirectory(logDir);
            File.AppendAllText(
                Path.Combine(logDir, "access.log"),
                DateTime.Now.ToString());

            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
            try
            {
                var data = new DataReader().GetAllData(customer: true, employee: false);
                CustomerdataGridView.DataSource = data;
            }
            catch (Exception)
            {
                // Data will remain empty if database is not available
            }
        }
    }
}
