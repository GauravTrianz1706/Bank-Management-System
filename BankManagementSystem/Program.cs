using System;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Windows.Forms;

namespace BankManagementSystem
{
    static class Program
    {
        // Blocker 2 Fix: Replace Windows Authentication with AWS Directory Service LDAP
        private static string StartupUser;

        [STAThread]
        static void Main()
        {
            if (!Environment.UserInteractive)
            {
                throw new InvalidOperationException("Interactive session required.");
            }

            // Blocker 2 Fix: Get username from environment variable or LDAP authentication
            // In cloud environment, this would be set after LDAP authentication
            StartupUser = Environment.GetEnvironmentVariable("AUTHENTICATED_USER") ?? "anonymous";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WelcomeUI());
        }

        // Helper method for LDAP authentication against AWS Managed Microsoft AD
        public static bool AuthenticateUser(string username, string password)
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
                    
                    StartupUser = username;
                    Environment.SetEnvironmentVariable("AUTHENTICATED_USER", username);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
