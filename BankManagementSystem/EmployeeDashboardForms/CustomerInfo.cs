// CustomerInfo.cs - Updated for .NET 8 compatibility
// CHANGES:
//   - Removed System.Messaging.MessageQueue usage entirely.
//     System.Messaging (MSMQ) is NOT available in .NET 8. The namespace was
//     removed from .NET Core/.NET 5+. Replaced with a structured log entry
//     using System.Diagnostics.Trace as a lightweight alternative.
//     For production use, replace with a proper logging framework (e.g., log4net,
//     Microsoft.Extensions.Logging, or a message broker like RabbitMQ/Azure Service Bus).
//   - Replaced hardcoded absolute path (C:\CustomerLogs\access.log) with a path
//     relative to AppContext.BaseDirectory per .NET 8 best practices.
//   - Added DataGridView population via DataReader.GetAllData() on form load.

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

            // REMOVED: System.Messaging.MessageQueue - not available in .NET 8.
            // REPLACEMENT: Use Trace or a proper logging/messaging framework.
            // Example with Trace (lightweight, no extra dependencies):
            System.Diagnostics.Trace.TraceInformation("Customer viewed");

            // CHANGED: Hardcoded absolute path replaced with AppContext.BaseDirectory
            string logDir = Path.Combine(AppContext.BaseDirectory, "CustomerLogs");
            Directory.CreateDirectory(logDir);
            File.AppendAllText(
                Path.Combine(logDir, "access.log"),
                DateTime.Now.ToString("o") + Environment.NewLine);

            // Populate the DataGridView with all customer data
            try
            {
                CustomerdataGridView.DataSource = new DataReader().GetAllData(customer: true, employee: false);
            }
            catch (Exception)
            {
                // If database is unavailable, leave the grid empty
            }
        }
    }
}
