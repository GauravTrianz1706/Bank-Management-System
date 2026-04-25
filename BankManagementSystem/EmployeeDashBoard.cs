using BankDatabaseAccess.EntityModel;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class EmployeeDashBoard : Form
    {
        private readonly PersonModel personModel;

    
        private static string CurrentEmployeeSession;

        
        
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
    }
}
