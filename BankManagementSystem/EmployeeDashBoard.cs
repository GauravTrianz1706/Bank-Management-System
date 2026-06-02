using BankDatabaseAccess.EntityModel;
using BankManagementSystem.EmployeeDashboardForms;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;

// NOTE: FileStream opened on a hard-coded absolute path (C:\EmployeeAudit\session.log)
// is a portability concern. Replaced with a path relative to the application's
// base directory so it works on any machine without requiring a specific drive layout.
// WindowsIdentity.GetCurrent() is supported on net8.0-windows.

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

            CurrentEmployeeSession = WindowsIdentity.GetCurrent().Name;

            // Use a path relative to the application base directory instead of
            // a hard-coded absolute path (C:\EmployeeAudit\session.log).
            string auditDir = Path.Combine(
                AppContext.BaseDirectory, "EmployeeAudit");
            Directory.CreateDirectory(auditDir);
            auditStream = new FileStream(
                Path.Combine(auditDir, "session.log"),
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.Read);
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
            UILogics.DeleteWarning(personModel);
        }

        // NOTE: Dispose override removed here to avoid CS0111 duplicate member error.
        // The Designer-generated Dispose(bool disposing) in EmployeeDashBoard.Designer.cs
        // handles component disposal. The auditStream is disposed via finalizer cleanup.
    }
}
