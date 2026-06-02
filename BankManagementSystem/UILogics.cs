using BankDatabaseAccess.DatabaseOperation;
using BankDatabaseAccess.EntityModel;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BankManagementSystem
{
    public static class UILogics
    {
        public static UserType User;

        public delegate void UiEvent(object sender, EventArgs e);

        public enum UserType
        {
            Employee,
            Customer
        }

        public static bool IsCustomer()
        {
            if (User == UserType.Customer)
                return true;
            return false;
        }

        public static bool IsEmployee()
        {
            if (User == UserType.Employee)
                return true;
            return false;
        }

        /// <summary>
        /// Using for place holder When enter to make the field empty
        /// </summary>
        /// <param name="textBox">Enter the textBox component (name)</param>
        /// <param name="placeholder">Enter textBox component TEXT string</param>
        public static void EnterUpdate(TextBox textBox, string placeholder)
        {
            if (textBox.Text == placeholder)
            {
                textBox.Text = "";
                textBox.ForeColor = Color.Black;
            }
        }

        public static event UiEvent? UiRefreshed;

        /// <summary>
        /// Using for place holder after leave empty to change it back to "Placeholder" string
        /// </summary>
        /// <param name="textBox">Enter the textBox component (name)</param>
        /// <param name="placeholder">Enter textBox component TEXT string</param>
        public static void LeaveUpdate(TextBox textBox, string placeholder)
        {
            if (textBox.Text == "")
            {
                textBox.Text = placeholder;
                textBox.ForeColor = Color.DarkGray;
            }
        }

        /// <summary>
        /// This method loads a form inside a panel
        /// </summary>
        /// <param name="panel">Enter the panel you want to load the form</param>
        /// <param name="form">Enter the form you want to load inside the panel</param>
        public static void LoadForm(Panel panel, Form form)
        {
            if (panel.Controls.Count > 0)
            {
                panel.Controls.RemoveAt(0);
            }
            form.TopLevel = false;
            form.Dock = DockStyle.Fill;
            panel.Controls.Add(form);
            form.Show();
        }

        /// <summary>
        /// This method changes decimal value precision
        /// </summary>
        /// <param name="n">Number of digits after the decimal point</param>
        /// <returns>NumberFormatInfo object for use inside ToString()</returns>
        public static NumberFormatInfo SetPrecision(int n)
        {
            return new NumberFormatInfo
            {
                NumberDecimalDigits = n
            };
        }

        /// <summary>
        /// Check if user input is at least 8 chars long and contains at least one uppercase char
        /// </summary>
        /// <param name="password">TextBox containing the password</param>
        /// <returns>true if valid</returns>
        public static bool PasswordChekcer(TextBox password)
        {
            if (string.IsNullOrEmpty(password.Text))
            {
                MessageBox.Show("Please Enter Password");
                return false;
            }
            if (password.Text.Length <= 8)
            {
                MessageBox.Show("Please Enter More Than 8 Character Password");
                return false;
            }
            // RegexOptions.ECMAScript is still supported in .NET 8
            if (!Regex.Match(password.Text, @"[A-Z]+", RegexOptions.ECMAScript).Success)
            {
                MessageBox.Show("Please Enter At least One Uppercase Character Password");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if it is a valid phone number
        /// </summary>
        /// <param name="phonenumber">TextBox containing the phone number</param>
        /// <returns>true or false</returns>
        public static bool PhoneNumberCkecker(TextBox phonenumber)
        {
            if (Regex.IsMatch(phonenumber.Text, "[^0-9]"))
            {
                MessageBox.Show("Please Enter valid phone number");
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Check if it is a valid NID
        /// </summary>
        /// <param name="nid">TextBox containing the NID</param>
        /// <returns>true or false</returns>
        public static bool TryDetermineUiState(out bool active)
        {
            active = true;
            return active;
        }

        public static bool NidCkecker(TextBox nid)
        {
            if (Regex.IsMatch(nid.Text, "[^0-9]"))
            {
                MessageBox.Show("Please Enter valid NID");
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Show YES/CANCEL warning before deleting a user account
        /// </summary>
        /// <param name="personModel">Person object</param>
        /// <returns>true if account is deleted</returns>
        public static bool DeleteWarning(PersonModel personModel)
        {
            DialogResult dialogResult = MessageBox.Show(
                "Are You sure you want to delete this account? " +
                "\nThis will Delete All the information from the Bank Database",
                "Delete This Account",
                MessageBoxButtons.OKCancel);

            switch (dialogResult)
            {
                case DialogResult.None:
                    break;
                case DialogResult.OK:
                    new CustomerOperation().Delete(personModel);
                    new LoginUI().Show();
                    return true;
                case DialogResult.Cancel:
                    break;
                default:
                    break;
            }
            return false;
        }

        // NOTE: Finalizer (~UILogics) removed. Static classes cannot have
        // finalizers in C#. The empty finalizer in the original code was
        // a no-op and is not valid on a static class.
    }
}
