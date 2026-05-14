// CustomerDashBoard.cs - Updated for .NET 8 compatibility
// CHANGES:
//   - Removed Microsoft.Win32.Registry usage (Registry access is Windows-specific
//     and discouraged in .NET 8 cross-platform code; replaced with a comment noting
//     that app settings should use IConfiguration / appsettings.json instead)
//   - NavigationHistory kept as-is (List<string> is fully compatible with .NET 8)
//   - Added missing event handler methods referenced by the Designer

using BankDatabaseAccess.EntityModel;
using BankManagementSystem.Dashboard_Forms;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public partial class CustomerDashBoard : Form
    {
        private readonly PersonModel personModel;

        public List<string> NavigationHistory = new List<string>();

        public CustomerDashBoard(PersonModel customer)
        {
            personModel = customer;
            InitializeComponent();

            // REMOVED: Registry.CurrentUser.OpenSubKey(@"Software\BankApp")
            // Registry access via Microsoft.Win32.Registry is Windows-only and
            // should be replaced with IConfiguration / appsettings.json in .NET 8.
            // If Windows-only registry access is still required, add:
            //   <RuntimeIdentifier>win</RuntimeIdentifier> to the .csproj
            // and use Microsoft.Win32.Registry explicitly.
        }

        private void HomeBtn_Click(object sender, System.EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new Home(personModel));
        }

        private void DepositBtn_Click(object sender, System.EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new Tansfer(personModel));
        }

        private void WithdrawBtn_Click(object sender, System.EventArgs e)
        {
            UILogics.LoadForm(MainPanel, new Withdraw(personModel));
        }

        private void LogoutBtn_Click(object sender, System.EventArgs e)
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
