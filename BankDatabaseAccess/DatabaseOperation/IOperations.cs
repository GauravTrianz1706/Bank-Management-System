using BankDatabaseAccess.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankDatabaseAccess.DatabaseOperation
{
    public interface IOperations
    {
        /// <summary>
        /// Insert data to database
        /// </summary>
        /// <param name="personModel">Customer or Employee Model</param>
        /// <returns></returns>
        int Insert(PersonModel personModel);
        
        /// <summary>
        /// Update data from database
        /// </summary>
        /// <param name="personModel">Customer or Employee Model</param>
        /// <returns></returns>
        int Update(PersonModel personModel);
        
        /// <summary>
        /// Delete data from database
        /// </summary>
        /// <param name="personModel">Customer or Employee Model</param>
        /// <returns></returns>
        int Delete(PersonModel personModel);
    }
}
