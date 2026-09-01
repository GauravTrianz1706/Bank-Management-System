using BankDatabaseAccess.EntityModel;
using System;
using System.IO;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class EmployeeDashBoard : Form
    {
        private readonly PersonModel personModel;

        // Replaced static Windows Identity session with instance-scoped field.
        // Azure AD / environment-based identity is used instead of WindowsIdentity.GetCurrent().
        private readonly string CurrentEmployeeSession;

        private FileStream auditStream;

        public EmployeeDashBoard(PersonModel personModel)
        {
            this.personModel = personModel;
            InitializeComponent();

            // Replaced WindowsIdentity.GetCurrent().Name (Windows Authentication) with
            // Azure AD-compatible identity sourced from environment variable.
            // In Azure, the authenticated user identity is propagated via claims/environment.
            CurrentEmployeeSession = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")
                ?? personModel.Username;

            // Replaced hard-coded Windows path (C:\EmployeeAudit\session.log) with
            // a configuration-driven, cross-platform path using Path.Combine.
            // The base directory is read from the AUDIT_LOG_BASE_PATH environment variable,
            // falling back to a relative path for local development.
            string auditBasePath = Environment.GetEnvironmentVariable("AUDIT_LOG_BASE_PATH")
                ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeAudit");

            if (!Directory.Exists(auditBasePath))
            {
                Directory.CreateDirectory(auditBasePath);
            }

            string auditFilePath = Path.Combine(auditBasePath, "session.log");
            auditStream = new FileStream(auditFilePath, FileMode.OpenOrCreate);
        }
    }
}
