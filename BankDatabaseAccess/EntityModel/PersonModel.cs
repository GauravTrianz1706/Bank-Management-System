using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankDatabaseAccess.EntityModel
{
    public  class PersonModel
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Eamil { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Nid { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
