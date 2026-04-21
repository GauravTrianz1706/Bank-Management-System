using BankDatabaseAccess.EntityModel;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class EmployeeDashBoard : Form
    {
        private readonly PersonModel personModel;

        // Stateful UI coupling (cloud)
        private static string CurrentSessionEmployee;

        [DllImport("user32.dll")]
        private static extern bool LockWorkStation();

        public EmployeeDashBoard(PersonModel personModel)
        {
            this.personModel = personModel;
            InitializeComponent();

            // Windows Authentication (cloud)
            CurrentSessionEmployee = WindowsIdentity.GetCurrent().Name;
        }

        private void DashBoard_Shown(object sender, EventArgs e)
        {
            HomeBtn.PerformClick();
        }

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            // Windows-only assumption (container)
            LockWorkStation();
            Close();
            new LoginUI().Show();
        }
    }
}
