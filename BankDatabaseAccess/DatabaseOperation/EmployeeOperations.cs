using BankDatabaseAccess.EntityModel;
using System;

namespace BankDatabaseAccess.DatabaseOperation
{
    public class EmployeeOperations : IOperations
    {
        /// <summary>
        /// This method is use for Employee Registration purpose 
        /// </summary>
        /// <param name="personModel">Take an Employee Object</param>
        /// <returns>Returns Row Number</returns>
        public int Insert(PersonModel personModel)
        {
            // PostgreSQL uses lowercase table/column names and public schema
            // Note: In production, use parameterized queries to prevent SQL injection
            var query = @"INSERT INTO public.employee(username,fullname,password,email,address,phone,nid,salary) 
                          VALUES ('" + personModel.Username + "'," +
                          "'" + personModel.FullName + "'," +
                          "'" + personModel.Password + "'," +
                          "'" + personModel.Email + "'," +
                          "'" + personModel.Address + "'," +
                          "'" + personModel.Phone + "'," +
                          "'" + personModel.Nid + "'," +
                          new Random().Next(30000, 1000000) + ")"; // generates Salary from 30k to 100k - numeric value without quotes
                          return DatabaseConnection.Execute(query);
        }
        /// <summary>
        /// This method is use for Deleting an existing Customer account form dashboard
        /// </summary>
        /// <param name="personModel">Takes an Customer Object</param>
        /// <returns>Returns Row Number</returns>
        public int Delete(PersonModel personModel)
        {
            // PostgreSQL uses lowercase table/column names
            var query = @"DELETE FROM public.customers 
                        WHERE username = '" + personModel.Username +"'";
            return DatabaseConnection.Execute(query);
        }
        /// <summary>
        /// THis method is use for update an existing Customer Account from dashboard
        /// </summary>
        /// <param name="personModel">Takes an Customer Object</param>
        /// <returns>Returns Row Number</returns>
        public int Update(PersonModel personModel)
        {
            // PostgreSQL uses lowercase table/column names
            // Fixed: Added missing space before WHERE clause
            var query = @"UPDATE public.customers SET 
                        email = '" + personModel.Email + "'," +
                        "phone = '" + personModel.Phone + "'," +
                        "address = '" + personModel.Address + "'," +
                        "nid = '" + personModel.Nid + "' " +
                        "WHERE username = '" + personModel.Username + "'";

            return DatabaseConnection.Execute(query);
        }
        /// <summary>
        /// Update Employee's data from db
        /// </summary>
        /// <param name="personModel">takes an employee object</param>
        /// <returns></returns>
        public int SelfUpdate(PersonModel personModel)
        {
            // PostgreSQL uses lowercase table/column names
            // Fixed: Added missing space before WHERE clause
            var query = @"UPDATE public.employee SET 
                        email = '" + personModel.Email + "'," +
                        "phone = '" + personModel.Phone + "'," +
                        "address = '" + personModel.Address + "'," +
                        "nid = '" + personModel.Nid + "' " +
                        "WHERE username = '"+ personModel.Username +"'";
            return DatabaseConnection.Execute(query);
        }
    }
}
