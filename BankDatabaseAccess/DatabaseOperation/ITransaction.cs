using BankDatabaseAccess.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankDatabaseAccess.DatabaseOperation
{
    public interface ITransaction
    {
        /// <summary>
        /// Update customer balance
        /// </summary>
        /// <param name="personModel">Take a Customer Object</param>
        /// <param name="amount">Take amount of money to deposit</param>
        int UpdateBalance(PersonModel personModel, decimal amount);
    }
}
