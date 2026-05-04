using BankDatabaseAccess.EntityModel;
using System;
using System.IO;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class EmployeeDashBoard : Form
    {
        private readonly PersonModel personModel;

    
        private static string CurrentEmployeeSession;

        
        
        private FileStream auditStream;

        public EmployeeDashBoard(PersonModel personModel)
        {
            this.personModel = personModel;
            InitializeComponent();

            // Fixed: Replaced Windows Authentication with environment variable
            CurrentEmployeeSession = Environment.GetEnvironmentVariable("EMPLOYEE_SESSION_ID") ?? "anonymous";

            // Fixed: Replaced hardcoded Windows path with environment variable
            string auditLogPath = Environment.GetEnvironmentVariable("AUDIT_LOG_PATH") ?? "/app/logs/session.log";
            auditStream = new FileStream(
                auditLogPath,
                FileMode.OpenOrCreate);
        }
    }
}
