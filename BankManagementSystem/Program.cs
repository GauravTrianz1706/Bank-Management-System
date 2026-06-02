using System;
using System.Security.Principal;
using System.Windows.Forms;

namespace BankManagementSystem
{
    static class Program
    {
        // NOTE: WindowsIdentity.GetCurrent() is supported on .NET 8 Windows,
        // but only when running on Windows. Since this is a net8.0-windows
        // WinForms app, this is acceptable.
        private static readonly string StartupUser =
            WindowsIdentity.GetCurrent().Name;

        [STAThread]
        static void Main()
        {
            if (!Environment.UserInteractive)
            {
                throw new InvalidOperationException("Interactive session required.");
            }

            // .NET 8 WinForms: ApplicationConfiguration.Initialize() replaces
            // EnableVisualStyles() + SetCompatibleTextRenderingDefault(false)
            ApplicationConfiguration.Initialize();
            Application.Run(new WelcomeUI());
        }
    }
}
