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

            // Fixed: Replaced hardcoded Windows path with environment variable
            string customerLogPath = Environment.GetEnvironmentVariable("CUSTOMER_LOG_PATH") ?? "/app/logs/access.log";
            File.AppendAllText(
                customerLogPath,
                System.DateTime.Now.ToString());
        }
    }
}
