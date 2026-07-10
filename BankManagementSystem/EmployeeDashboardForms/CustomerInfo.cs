using System;
using System.IO;
using System.Messaging;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class CustomerInfo : Form
    {
        public CustomerInfo()
        {
            InitializeComponent();

            
            var queue = new MessageQueue(@".\Private$\customer-info");
            queue.Send("Customer viewed");

            // CONTAINERIZATION FIX: Replaced hardcoded Windows path (C:\CustomerLogs\access.log)
            // with cross-platform path using environment variable and Path.Combine
            // Blocker IDs: blocker-2 (cz-dotnet-0001 - Windows-Specific Paths),
            //              blocker-7 (cz-dotnet-0006 - Drive Letter Dependencies)
            // In production, mount Azure Files or Blob Storage via CSI drivers for persistent logs
            string customerLogPath = Environment.GetEnvironmentVariable("CUSTOMER_LOG_PATH") 
                ?? Path.Combine(Path.GetTempPath(), "CustomerLogs");
            
            // Ensure directory exists
            if (!Directory.Exists(customerLogPath))
            {
                Directory.CreateDirectory(customerLogPath);
            }

            File.AppendAllText(
                Path.Combine(customerLogPath, "access.log"),
                System.DateTime.Now.ToString());
        }
    }
}
