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
            // Use a simple file-based audit log instead.
            string logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BankApp", "CustomerLogs");
            Directory.CreateDirectory(logDir);
            File.AppendAllText(
                Path.Combine(logDir, "access.log"),
                DateTime.Now.ToString() + Environment.NewLine);

            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
            try
            {
                CustomerdataGridView.DataSource = new DataReader().GetAllData(customer: true, employee: false);
            }
            catch (Exception)
            {
                // Data load failed - grid will remain empty
            }
        }
    }
}
