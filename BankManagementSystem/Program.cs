using System;
using System.Windows.Forms;
// Blocker-2 (cr-dotnet-0030): Removed System.Security.Principal / WindowsIdentity import.
// Windows Authentication (NTLM/Kerberos) replaced with Google Cloud Workforce Identity Federation.
// The startup user identity is now obtained from the GCP_WORKFORCE_USER_IDENTITY environment
// variable injected by GKE / Workforce Identity Federation at runtime.

namespace BankManagementSystem
{
    static class Program
    {
        // Blocker-2 (cr-dotnet-0030): Replace WindowsIdentity.GetCurrent().Name with
        // GCP Workforce Identity Federation environment variable.
        // GKE injects the authenticated principal via GCP_WORKFORCE_USER_IDENTITY.
        private static readonly string StartupUser =
            Environment.GetEnvironmentVariable("GCP_WORKFORCE_USER_IDENTITY")
            ?? "unknown-user";

        [STAThread]
        static void Main()
        {
            if (!Environment.UserInteractive)
            {
                throw new InvalidOperationException("Interactive session required.");
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WelcomeUI());
        }
    }
}
