using System;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace BankManagementSystem
{
    static class Program
    {
        private static readonly string StartupUser =
            WindowsIdentity.GetCurrent().Name;

        public static IConfiguration? Configuration { get; private set; }

        [STAThread]
        static void Main()
        {
            if (!Environment.UserInteractive)
            {
                throw new InvalidOperationException("Interactive session required.");
            }

            // Load configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            
            Configuration = builder.Build();

            // Set connection string in DatabaseConnection
            var connectionString = Configuration.GetConnectionString("OpenBankLocal");
            if (!string.IsNullOrEmpty(connectionString))
            {
                BankDatabaseAccess.DatabaseConnection.Connection = connectionString;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WelcomeUI());
        }
    }
}
