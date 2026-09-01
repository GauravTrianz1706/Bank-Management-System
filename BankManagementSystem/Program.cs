using System;
using System.Windows.Forms;

namespace BankManagementSystem
{
    static class Program
    {
        // Replaced WindowsIdentity.GetCurrent().Name (Windows Authentication) with
        // Azure AD-compatible identity sourced from environment variable.
        // This enables cloud-based authentication for non-domain scenarios.
        private static readonly string StartupUser =
            Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ?? "UnknownUser";

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
