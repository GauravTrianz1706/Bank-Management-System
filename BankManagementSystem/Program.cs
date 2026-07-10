using System;
using System.Windows.Forms;

namespace BankManagementSystem
{
    static class Program
    {
        // CONTAINERIZATION FIX: Replaced Windows Authentication (WindowsIdentity.GetCurrent().Name)
        // with environment variable for cross-platform compatibility
        // Blocker ID: blocker-11 (cz-dotnet-0036 - Windows Authentication)
        // In production, use Azure AD (Entra ID) authentication with Microsoft.Identity.Web
        // for cloud-native identity management that works across Windows/Linux containers
        private static readonly string StartupUser =
            Environment.GetEnvironmentVariable("STARTUP_USER") 
            ?? Environment.UserName;

        private static HealthCheckEndpoint _healthCheck;

        [STAThread]
        static void Main()
        {
            // Start health check endpoint for containerization
            _healthCheck = new HealthCheckEndpoint();
            _healthCheck.Start();

            if (!Environment.UserInteractive)
            {
                throw new InvalidOperationException("Interactive session required.");
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Ensure health check is stopped when application exits
            Application.ApplicationExit += (s, e) => _healthCheck?.Stop();
            
            Application.Run(new WelcomeUI());
        }
    }
}
