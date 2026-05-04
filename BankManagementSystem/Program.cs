using System;
using System.Windows.Forms;

namespace BankManagementSystem
{
    static class Program
    {
        // Fixed: Replaced Windows Authentication with environment variable
        private static readonly string StartupUser =
            Environment.GetEnvironmentVariable("STARTUP_USER") ?? "anonymous";

        private static HealthCheckEndpoint _healthCheck;

        [STAThread]
        static void Main()
        {
            if (!Environment.UserInteractive)
            {
                throw new InvalidOperationException("Interactive session required.");
            }

            // Start health check endpoint for containerization
            _healthCheck = new HealthCheckEndpoint();
            _healthCheck.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WelcomeUI());

            // Stop health check on application exit
            _healthCheck.Stop();
        }
    }
}
