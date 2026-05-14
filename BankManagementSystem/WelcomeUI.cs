// WelcomeUI.cs - Updated for .NET 8 compatibility
// CHANGES:
//   - Updated Process.Start() call to use ProcessStartInfo with UseShellExecute = true.
//     In .NET Core/.NET 5+, Process.Start(string) for URLs requires UseShellExecute = true
//     explicitly; the default changed from true to false in .NET Core.
//     Without this fix, opening a URL via Process.Start throws an exception on .NET 8.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class WelcomeUI : Form
    {
        public WelcomeUI()
        {
            InitializeComponent();
        }

        private void AppCloseLbl_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void GetStartedBtn_Click(object sender, EventArgs e)
        {
            UILogics.User = UILogics.UserType.Customer;
            this.Hide();
            new LoginUI().Show();
        }

        private void EmpLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UILogics.User = UILogics.UserType.Employee;
            this.Hide();
            new LoginUI().Show();
        }

        private void GetStartedBtn_MouseHover(object sender, EventArgs e)
        {
            GetStartedBtn.Size = new Size(177, 67);
        }

        private void GetStartedBtn_MouseLeave(object sender, EventArgs e)
        {
            GetStartedBtn.Size = new Size(175, 65);
        }

        private void GitBtn_Click(object sender, EventArgs e)
        {
            // CHANGED: Process.Start(string) → Process.Start(ProcessStartInfo) with
            // UseShellExecute = true. Required for URL launching in .NET 5+/.NET 8.
            // In .NET Core, UseShellExecute defaults to false, which causes an exception
            // when trying to open a URL without specifying a shell executable.
            Process.Start(new ProcessStartInfo(
                "https://github.com/b14ck0ps/Bank-Management-System")
            {
                UseShellExecute = true
            });
        }
    }
}
