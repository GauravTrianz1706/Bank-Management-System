using BankDatabaseAccess.EntityModel;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class EmployeeDashBoard : Form
    {
        private readonly PersonModel personModel;

    
        private static string CurrentEmployeeSession = string.Empty;

        
        
        private FileStream auditStream;

        public EmployeeDashBoard(PersonModel personModel)
        {
            this.personModel = personModel;
            InitializeComponent();

            
            CurrentEmployeeSession = WindowsIdentity.GetCurrent().Name;

            auditStream = new FileStream(
                @"C:\EmployeeAudit\session.log",
                FileMode.OpenOrCreate);
        }

        private void HomeBtn_Click_1(object? sender, System.EventArgs e)
        {
            // Placeholder for HomeBtn_Click_1 event handler
        }

        private void CustomerInfoBtn_Click(object? sender, System.EventArgs e)
        {
            // Placeholder for CustomerInfoBtn_Click event handler
        }

        private void EditInfoBtn_Click(object? sender, System.EventArgs e)
        {
            // Placeholder for EditInfoBtn_Click event handler
        }

        private void LogoutBtn_Click_1(object? sender, System.EventArgs e)
        {
            // Placeholder for LogoutBtn_Click_1 event handler
        }

        private void DepositBtn_Click(object? sender, System.EventArgs e)
        {
            // Placeholder for DepositBtn_Click event handler
        }

        private void DeleteLnk_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            // Placeholder for DeleteLnk_LinkClicked event handler
        }
    }
}
