using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankDatabaseAccess.EntityModel
{
    public class PersonModel
    {
        /// <summary>
        /// This is the Person Username. Value cannot be null.
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// This is the person Full name. Value cannot be null.
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// This is the Person Password. Value cannot be null.
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// This is the person Email. Value cannot be null.
        /// </summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// This is person phone number
        /// </summary>
        public string Phone { get; set; } = string.Empty;
        /// <summary>
        /// This is person National ID number. Value cannot be null.
        /// </summary>
        public string Nid { get; set; } = string.Empty;
        /// <summary>
        /// This is person Address. Value cannot be null.
        /// </summary>
        public string Address { get; set; } = string.Empty;
    }
}
