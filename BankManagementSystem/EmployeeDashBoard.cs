using BankDatabaseAccess.EntityModel;
using BankManagementSystem.EmployeeDashboardForms;
using System;
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

            // Use environment username instead of WindowsIdentity for cross-platform compatibility
            CurrentEmployeeSession = Environment.UserName;

            // Use application-relative path instead of hardcoded absolute path
            string auditDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BankApp", "EmployeeAudit");
            Directory.CreateDirectory(auditDir);
            auditStream = new FileStream(
                Path.Combine(auditDir, "session.log"),
                FileMode.OpenOrCreate);

            HomeBtn_Click_1(this, EventArgs.Empty);
        }

        private void HomeBtn_Click_1(object sender, EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new Home(personModel));
        }

        private void CustomerInfoBtn_Click(object sender, EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new CustomerInfo());
        }

        private void EditInfoBtn_Click(object sender, EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new EditInfo());
        }

        private void DepositBtn_Click(object sender, EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new Deposit());
        }

        private void LogoutBtn_Click_1(object sender, EventArgs e)
        {
            auditStream?.Close();
            this.Close();
            new WelcomeUI().Show();
        }

        private void DeleteLnk_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (UILogics.DeleteWarning(personModel))
            {
                auditStream?.Close();
                this.Close();
            }
        }
    }
}
