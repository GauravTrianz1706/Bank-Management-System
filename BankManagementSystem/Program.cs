using System;
using System.Windows.Forms;

namespace BankManagementSystem
{
    static class Program
    {
        // Fixed: Replaced Windows Authentication with environment variable
        private static readonly string StartupUser =
            Environment.GetEnvironmentVariable("CURRENT_USER") ?? "anonymous";

        private static HealthCheckEndpoint _healthCheckEndpoint;

        [STAThread]
        static void Main()
        {
            if (!Environment.UserInteractive)
            {
                throw new InvalidOperationException("Interactive session required.");
            }

            // Start health check endpoint for containerization support
            int healthCheckPort = int.TryParse(Environment.GetEnvironmentVariable("HEALTH_CHECK_PORT"), out int port) ? port : 8080;
            _healthCheckEndpoint = new HealthCheckEndpoint(healthCheckPort);
            _healthCheckEndpoint.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WelcomeUI());

            // Cleanup health check endpoint on exit
            _healthCheckEndpoint?.Stop();
        }
    }
}
