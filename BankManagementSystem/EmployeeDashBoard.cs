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

        private FileStream? auditStream;

        public EmployeeDashBoard(PersonModel personModel)
        {
            this.personModel = personModel;
            InitializeComponent();

            // Track current employee session
            CurrentEmployeeSession = WindowsIdentity.GetCurrent().Name;

            // Initialize audit stream
            try
            {
                var auditDir = @"C:\EmployeeAudit";
                if (!Directory.Exists(auditDir))
                {
                    Directory.CreateDirectory(auditDir);
                }
                auditStream = new FileStream(
                    Path.Combine(auditDir, "session.log"),
                    FileMode.OpenOrCreate);
            }
            catch
            {
                // Ignore audit stream errors
                auditStream = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                auditStream?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
