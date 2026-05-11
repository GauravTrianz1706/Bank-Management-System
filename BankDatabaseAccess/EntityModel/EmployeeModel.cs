using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankDatabaseAccess.EntityModel
{
    public class EmployeeModel : PersonModel
    {
        /// <summary>
        /// This is Employee's Salary. This value is a get only value. This value cannot be set by any Users.
        /// This value gets the Salary from the database.
        /// </summary>
        public decimal Salary { get; private set; }
    }
}
