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

            // CONTAINERIZATION FIX: Replaced Windows Authentication (WindowsIdentity.GetCurrent().Name)
            // with environment variable for cross-platform compatibility
            // Blocker IDs: blocker-10 (cz-dotnet-0036 - Windows Authentication)
            // In production, use Azure AD authentication with Microsoft.Identity.Web
            CurrentEmployeeSession = Environment.GetEnvironmentVariable("CURRENT_USER") 
                ?? Environment.UserName;

            // CONTAINERIZATION FIX: Replaced hardcoded Windows path (C:\EmployeeAudit\session.log)
            // with cross-platform path using environment variable and Path.Combine
            // Blocker IDs: blocker-1 (cz-dotnet-0001 - Windows-Specific Paths), 
            //              blocker-6 (cz-dotnet-0006 - Drive Letter Dependencies)
            // In production, mount Azure Files or Blob Storage via CSI drivers
            string auditLogPath = Environment.GetEnvironmentVariable("AUDIT_LOG_PATH") 
                ?? Path.Combine(Path.GetTempPath(), "EmployeeAudit");
            
            // Ensure directory exists
            if (!Directory.Exists(auditLogPath))
            {
                Directory.CreateDirectory(auditLogPath);
            }

            auditStream = new FileStream(
                Path.Combine(auditLogPath, "session.log"),
                FileMode.OpenOrCreate);
        }
    }
}
