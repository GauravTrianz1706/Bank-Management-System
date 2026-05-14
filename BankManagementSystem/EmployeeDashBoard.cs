// EmployeeDashBoard.cs - Updated for .NET 8 compatibility
// CHANGES:
//   - Removed System.Security.Principal.WindowsIdentity usage for session tracking
//     (replaced with Environment.UserName which is cross-platform compatible)
//   - Replaced hardcoded absolute path FileStream (C:\EmployeeAudit\session.log)
//     with a path relative to the application's base directory using
//     AppContext.BaseDirectory, which is the .NET 8 recommended approach
//   - FileStream is still valid in .NET 8; only the hardcoded path was changed
//   - Added missing event handler methods referenced by the Designer

using BankDatabaseAccess.EntityModel;
using BankManagementSystem.EmployeeDashboardForms;
using System.IO;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class EmployeeDashBoard : Form
    {
        private readonly PersonModel personModel;

        private static string? CurrentEmployeeSession;

        private FileStream? auditStream;

        public EmployeeDashBoard(PersonModel personModel)
        {
            this.personModel = personModel;
            InitializeComponent();

            // CHANGED: WindowsIdentity.GetCurrent().Name → Environment.UserName
            // Environment.UserName is cross-platform and works on .NET 8 without
            // requiring the Windows Compatibility Pack.
            CurrentEmployeeSession = System.Environment.UserName;

            // CHANGED: Hardcoded absolute path replaced with AppContext.BaseDirectory
            // to avoid platform-specific path issues on .NET 8.
            string auditDir = Path.Combine(AppContext.BaseDirectory, "EmployeeAudit");
            Directory.CreateDirectory(auditDir);
            auditStream = new FileStream(
                Path.Combine(auditDir, "session.log"),
                FileMode.OpenOrCreate);
        }

        private void HomeBtn_Click_1(object sender, System.EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new Home(personModel));
        }

        private void CustomerInfoBtn_Click(object sender, System.EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new CustomerInfo());
        }

        private void EditInfoBtn_Click(object sender, System.EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new EditInfo());
        }

        private void DepositBtn_Click(object sender, System.EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new Deposit());
        }

        private void LogoutBtn_Click_1(object sender, System.EventArgs e)
        {
            this.Close();
            new WelcomeUI().Show();
        }

        private void DeleteLnk_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DeleteLnk.Visible = false;
            DeleteAccountLbl.Visible = false;
            UILogics.DeleteWarning(personModel);
        }
    }
}
