using BankDatabaseAccess.EntityModel;
using System;
using System.IO;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class EmployeeDashBoard : Form
    {
        private readonly PersonModel personModel;

        // Blocker 14 Fix: Remove static state - use instance field instead
        private string currentEmployeeSession;

        // Blocker 7 Fix: Use environment variable for audit log path
        private FileStream auditStream;

        public EmployeeDashBoard(PersonModel personModel)
        {
            this.personModel = personModel;
            InitializeComponent();

            // Blocker 1 Fix: Replace Windows Authentication with LDAP authentication
            // Get authenticated user from environment variable set during LDAP authentication
            currentEmployeeSession = Environment.GetEnvironmentVariable("AUTHENTICATED_USER") ?? "anonymous";

            // Blocker 7 Fix: Replace hard-coded Windows path with environment variable and Path.Combine
            string auditLogPath = Environment.GetEnvironmentVariable("AUDIT_LOG_PATH") ?? "/var/log/bank";
            string auditLogFile = Path.Combine(auditLogPath, "session.log");
            
            // Ensure directory exists
            Directory.CreateDirectory(auditLogPath);
            
            auditStream = new FileStream(
                auditLogFile,
                FileMode.OpenOrCreate);
        }

        // Helper method for LDAP authentication against AWS Managed Microsoft AD
        private bool AuthenticateEmployee(string username, string password)
        {
            try
            {
                string ldapServer = Environment.GetEnvironmentVariable("LDAP_SERVER") ?? "localhost";
                int ldapPort = int.Parse(Environment.GetEnvironmentVariable("LDAP_PORT") ?? "389");
                string ldapBaseDn = Environment.GetEnvironmentVariable("LDAP_BASE_DN") ?? "dc=example,dc=com";

                using (var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapServer, ldapPort)))
                {
                    connection.AuthType = AuthType.Basic;
                    connection.Credential = new NetworkCredential($"cn={username},{ldapBaseDn}", password);
                    connection.Bind();
                    
                    currentEmployeeSession = username;
                    Environment.SetEnvironmentVariable("AUTHENTICATED_USER", username);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                auditStream?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
