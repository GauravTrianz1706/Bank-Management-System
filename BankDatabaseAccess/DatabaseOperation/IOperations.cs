using BankDatabaseAccess.EntityModel;

namespace BankDatabaseAccess.DatabaseOperation
{
    public interface IOperations
    {
        /// <summary>
        /// Insert Data to the database
        /// </summary>
        /// <param name="personModel">Customer or Employee Model</param>
        /// <returns></returns>
        int Insert(PersonModel personModel);
        /// <summary>
        /// Update data from databse
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
