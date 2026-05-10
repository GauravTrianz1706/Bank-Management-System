using System;
using System.Windows.Forms;

namespace BankManagementSystem
{
    static class Program
    {
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
