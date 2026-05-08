using BankDatabaseAccess.EntityModel;
using System;

namespace BankDatabaseAccess.DatabaseOperation
{
    
    public class CustomerOperation : IOperations , ITransaction
    {
        private const decimal InitialBalance = 100;
        /// <summary>
        /// This method is use for Customers Registration Purpose.
        /// </summary>
        /// <param name="personModel">Take a Customer Object</param>
        /// <returns>Return Row Number</returns>
        public int Insert(PersonModel personModel)
        {
            // PostgreSQL uses lowercase table/column names and public schema
            // Note: In production, use parameterized queries to prevent SQL injection
            var query = @"INSERT INTO public.customers(username,fullname,password,email,phone,nid,address,balance) 
                          VALUES ('" + personModel.Username + "'," +
                          "'" + personModel.FullName + "'," +
                          "'" + personModel.Password + "'," +
                          "'" + personModel.Email + "'," +
                          "'" + personModel.Phone + "'," +
                          "'" + personModel.Nid + "'," +
                          "'" + personModel.Address + "'," +
                          InitialBalance + ")"; // Set Opening Balance 100 for all customers
            return DatabaseConnection.Execute(query);
        }
        /// <summary>
        /// Delete a user from data base
        /// </summary>
        /// <param name="personModel">Take a Customer Object</param>
        /// <returns>row effect</returns>
        public int Delete(PersonModel personModel)
        {
           return new EmployeeOperations().Delete(personModel);
        }

        /// <summary>
        /// Update a user form data base
        /// </summary>
        /// <param name="personModel">Take a Customer Object</param>
        /// <returns>row effect</returns>
        public int Update(PersonModel personModel)
        {
            // PostgreSQL uses lowercase table/column names
            // Fixed WHERE clause - moved username comparison to the right side
            var query = @"UPDATE public.customers SET 
                         email = '" + personModel.Email + "'," +
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
            // PostgreSQL uses lowercase table/column names
            // Numeric values should not be quoted in PostgreSQL
            var query = @"UPDATE public.customers SET 
                        balance = " + amount +
                         " WHERE username = '" + personModel.Username +"'";
            return DatabaseConnection.Execute(query);
        }
    }
}
