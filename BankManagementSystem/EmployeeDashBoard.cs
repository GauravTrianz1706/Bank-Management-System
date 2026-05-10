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

            // Use current user's environment-based path instead of Windows-specific identity
            CurrentEmployeeSession = System.Environment.UserName;

            // Use environment-agnostic path for audit log
            string auditDir = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                "BankApp", "EmployeeAudit");
            Directory.CreateDirectory(auditDir);
            auditStream = new FileStream(
                Path.Combine(auditDir, "session.log"),
                FileMode.OpenOrCreate);

            UILogics.LoadForm(MainPanel, new Home(personModel));
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
