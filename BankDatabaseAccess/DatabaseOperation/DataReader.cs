using Npgsql;
using System.Data;

namespace BankDatabaseAccess.DatabaseOperation
{
    public class DataReader
    {
        private string query = "--";

        private DataTable DataTable()
        {
            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, DatabaseConnection.Connection);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }

        /// <summary>
        /// This method read data from the data base
        /// </summary>
        /// <param name="personModel">Enter a person model ie. Employee or Customer</param>
        /// <param name="customer">This is true if customer data needed</param>
        /// <param name="employee">This is true if employee data needed</param>
        /// <returns>This return a datatable of the given object from the database</returns>
        public DataTable GetSingleData(EntityModel.PersonModel personModel, bool customer, bool employee)
        {
            query = @"SELECT * 
                        FROM public." + Table(customer, employee) + " " +
                        "WHERE username = '" + personModel.Username + "'";
            return DataTable();
        }

        /// <summary>
        /// This method read the whole data from a table except Passwords
        /// </summary>
        /// <param name="customer">Set True if Customer data table needed</param>
        /// <param name="employee">Set True if Employee data needed</param>
        /// <returns>Return all Columns and rows from the table</returns>
        public DataTable GetAllData(bool customer = false, bool employee = false)
        {
            query = @"SELECT 
                         full_name AS ""Full Name""
                        ,email AS ""Email Address""
                        ,phone AS ""Phone Number""
                        ,nid AS ""National ID""
                        ,balance AS balance
                        ,address AS address
                        ,join_date AS ""Account Created""
                    FROM public." + Table(customer, employee);
            return DataTable();
        }

        /// <summary>
        /// This declares from which table data will be read
        /// </summary>
        /// <param name="customer">True if customer table needed</param>
        /// <param name="employee">True if employee table needed</param>
        /// <returns>Return The Table name as a string</returns>
        private static string? Table(bool customer, bool employee)
        {
            if (customer)
                return "customers";
            if (employee)
                return "employee";
            return null;
        }
    }
}
