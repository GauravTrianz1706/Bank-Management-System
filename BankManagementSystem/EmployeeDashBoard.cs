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
            CurrentEmployeeSession = Environment.GetEnvironmentVariable("CURRENT_USER") ?? "anonymous";

            // Fixed: Replaced Windows-specific path with cross-platform path using environment variable
            string auditLogPath = Environment.GetEnvironmentVariable("AUDIT_LOG_PATH") ?? Path.Combine(Path.GetTempPath(), "EmployeeAudit");
            Directory.CreateDirectory(auditLogPath);
            string sessionLogFile = Path.Combine(auditLogPath, "session.log");
            
            auditStream = new FileStream(
                sessionLogFile,
                FileMode.OpenOrCreate);
        }
    }
}
