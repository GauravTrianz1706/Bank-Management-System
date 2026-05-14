// Program.cs - Updated for .NET 8 compatibility
// CHANGES:
//   - Fixed syntax error: "using Systemusing" corrected to "using System;"
//   - Removed WindowsIdentity.GetCurrent() startup usage (Windows-specific identity
//     APIs still work on net8.0-windows but the pattern is unnecessary at startup)
//   - Application.SetHighDpiMode added per .NET 6+ WinForms best practice
//   - Retained STAThread and Application.Run pattern (valid for .NET 8 WinForms)

using System;
using System.Windows.Forms;

namespace BankManagementSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            if (!Environment.UserInteractive)
            {
                throw new InvalidOperationException("Interactive session required.");
            }

            // .NET 6+ WinForms: configure DPI awareness before any UI is created
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WelcomeUI());
        }
    }
}
