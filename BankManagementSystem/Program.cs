using System;using Systemusing System.Security.Principal;
using System.Windows.Forms;

namespace BankManagementSystem
{
    static class Program
    {
        private static readonly string StartupUser =
            WindowsIdentity.GetCurrent().Name;

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
