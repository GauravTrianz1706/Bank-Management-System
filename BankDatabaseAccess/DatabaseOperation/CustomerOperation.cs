using BankDatabaseAccess.EntityModel;

namespace BankDatabaseAccess.DatabaseOperation
{
    public class CustomerOperation : IOperations, ITransaction
    {
        private const decimal InitialBalance = 100;

        /// <summary>
        /// This method is use for Customers Registration Purpose.
        /// </summary>
        /// <param name="personModel">Take a Customer Object</param>
        /// <returns>Return Row Number</returns>
        public int Insert(PersonModel personModel)
        {
            var query = @"INSERT INTO public.customers(username, full_name, password, email, phone, nid, address, balance) 
                          VALUES ('" + personModel.Username + "'," +
                          "'" + personModel.FullName + "'," +
                          "'" + personModel.Password + "'," +
                          "'" + personModel.Eamil + "'," +
                          "'" + personModel.Phone + "'," +
                          "'" + personModel.Nid + "'," +
                          "'" + personModel.Address + "'," +
                          "'" + InitialBalance + "')"; // Set Opening Balance 100 for all customers
            return DatabaseConnection.Execute(query);
        }

        /// <summary>
        /// Delete a user from database
        /// </summary>
        /// <param name="personModel">Take a Customer Object</param>
        /// <returns>row effect</returns>
        public int Delete(PersonModel personModel)
        {
            return new EmployeeOperations().Delete(personModel);
        }

        /// <summary>
        /// Update a user from database
        /// </summary>
        /// <param name="personModel">Take a Customer Object</param>
        /// <returns>row effect</returns>
        public int Update(PersonModel personModel)
        {
            var query = @"UPDATE public.customers SET 
                         email = '" + personModel.Eamil + "'," +
                         "phone = '" + personModel.Phone + "'," +
                         "nid = '" + personModel.Nid + "'," +
                         "address = '" + personModel.Address + "'" +
                         " WHERE username = '" + personModel.Username + "'";
            return DatabaseConnection.Execute(query);
        }

        /// <summary>
        /// Update customer balance
        /// </summary>
        /// <param name="personModel">Take a Customer Object</param>
        /// <param name="amount"></param>
        /// <returns>row effected</returns>
        public int UpdateBalance(PersonModel personModel, decimal amount)
        {
            var query = @"UPDATE public.customers SET 
                        balance = '" + amount + "'" +
                         " WHERE username = '" + personModel.Username + "'";
            return DatabaseConnection.Execute(query);
        }
    }
}
