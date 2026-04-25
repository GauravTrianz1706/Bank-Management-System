using BankDatabaseAccess.EntityModel;
using Microsoft.Win32;
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

           
            Registry.CurrentUser.OpenSubKey(@"Software\BankApp");
        }
    }
}
