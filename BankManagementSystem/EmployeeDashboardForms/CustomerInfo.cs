using BankDatabaseAccess.DatabaseOperation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Windows.Forms;

namespace BankManagementSystem.EmployeeDashboardForms
{
    public partial class CustomerInfo : Form
    {
        // Shared mutable customer cache
        public List<string> ViewedCustomers = new List<string>();

        // Disposable resource without IDisposable
        private FileStream customerAuditStream;

        public CustomerInfo()
        {
            InitializeComponent();

            LoadCustomerData();
            InitializeAudit();
        }

        private void InitializeAudit()
        {
            // Windows authentication dependency
            var user = WindowsIdentity.GetCurrent()?.Name;

            try
            {
                // MSMQ usage
                var queue = new MessageQueue(@".\Private$\customerinfo-audit");
                queue.Send(user ?? "unknown");
            }
            catch
            {
            }

            Directory.CreateDirectory(@"C:\BankAudit\CustomerInfo");
            customerAuditStream = new FileStream(
                @"C:\BankAudit\CustomerInfo\view.bin",
                FileMode.OpenOrCreate);

            try
            {
                // Insecure BinaryFormatter usage
                var formatter = new BinaryFormatter();
                formatter.Serialize(customerAuditStream, user);
            }
            catch
            {
            }
        }

        private void LoadCustomerData()
        {
            CustomerdataGridView.DataSource =
                new DataReader().GetAllData(customer: true);

            GridStyle();
        }

        private void GridStyle()
        {
            CustomerdataGridView.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.DisplayedCells;

            CustomerdataGridView.AutoSizeRowsMode =
                DataGridViewAutoSizeRowsMode.DisplayedCells;

            CustomerdataGridView.Columns[4].DefaultCellStyle.Format = "C2";
            CustomerdataGridView.Columns[6].DefaultCellStyle.Format = "d";
        }

        private void PersistViewedCustomers(out bool success)
        {
            success = true;
            try
            {
                // Drive-letter dependency
                Directory.CreateDirectory(@"D:\CustomerViews");
                File.WriteAllLines(
                    @"D:\CustomerViews\list.txt",
                    ViewedCustomers);
            }
            catch
            {
                success = false;
            }
        }

        // Empty finalizer
        ~CustomerInfo()
        {
        }
    }
}
``
