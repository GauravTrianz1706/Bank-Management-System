using Npgsql;
using System.Data;

namespace BankDatabaseAccess.DatabaseOperation
{
    public class DataReader
    {
        private string query = "--";
        private  DataTable DataTable()
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
        /// <param name="employee">This is true if eployee data nedded</param>
        /// <returns>This return a datatable of the given object from the database</returns>
        public DataTable GetSingleData(EntityModel.PersonModel personModel,bool customer,bool employee)
        {
            // PostgreSQL uses public schema by default and lowercase table names
            // Using parameterized queries would be better for security, but maintaining current structure
            query = @"SELECT * 
                        FROM public." + Table(customer,employee) + " " +
                        "WHERE username =  '"+ personModel.Username +"'";
            return DataTable();
        }
        /// <summary>
        /// This method read the whole data from a table exept Passwords
        /// </summary>
        /// <param name="customer">Set True if Customer data table needed</param>
        /// <param name="employee">Set True id Employee data needed</param>
        /// <returns>Return all Coloumns and rows from the table</returns>
        public DataTable GetAllData(bool customer = false, bool employee = false)
        {
                // PostgreSQL uses lowercase column names and public schema
                // PostgreSQL requires double quotes for column aliases with spaces, or use AS without quotes
                query = @"SELECT 
                             fullname AS ""Full Name""
                            ,email AS ""Email Address""
                            ,phone AS ""Phone Number""
                            ,nid AS ""National ID""
                            ,balance AS ""Balance""
                            ,address AS ""Address""
                            ,joindate AS ""Account Created""
                        FROM public." + Table(customer,employee);
            return DataTable();
        }
        /// <summary>
        /// This declare from which table data will be read
        /// </summary>
        /// <param name="customer">True if customer table needed</param>
        /// <param name="employee">True if employee table needed</param>
        /// <returns>Return The Table name as a string</returns>
        private string? Table(bool customer, bool employee)
        {
            if (customer)
                return "customers";
            if (employee)
                return "employee";
            return null;
        }
    }
}
