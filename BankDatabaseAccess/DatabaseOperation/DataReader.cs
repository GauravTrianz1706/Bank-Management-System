using Npgsql;
using System.Data;

namespace BankDatabaseAccess.DatabaseOperation
{
    public class DataReader
    {
        /// <summary>
        /// This method read data from the data base using parameterized queries.
        /// </summary>
        /// <param name="personModel">Enter a person model ie. Employee or Customer</param>
        /// <param name="customer">This is true if customer data needed</param>
        /// <param name="employee">This is true if employee data needed</param>
        /// <returns>This return a datatable of the given object from the database</returns>
        public DataTable GetSingleData(EntityModel.PersonModel personModel, bool customer, bool employee)
        {
            string tableName = Table(customer, employee) ?? throw new InvalidOperationException("Either customer or employee must be true.");
            // Table name is a controlled internal value (not user input), safe to interpolate.
            string query = $"SELECT * FROM public.{tableName} WHERE username = @username";

            using (var connection = new NpgsqlConnection(DatabaseConnection.Connection))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", personModel.Username ?? string.Empty);
                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        /// <summary>
        /// This method read the whole data from a table except Passwords.
        /// </summary>
        /// <param name="customer">Set True if Customer data table needed</param>
        /// <param name="employee">Set True if Employee data needed</param>
        /// <returns>Return all Columns and rows from the table</returns>
        public DataTable GetAllData(bool customer = false, bool employee = false)
        {
            string tableName = Table(customer, employee) ?? throw new InvalidOperationException("Either customer or employee must be true.");
            // Table name is a controlled internal value (not user input), safe to interpolate.
            string query = $@"SELECT
                             full_name  AS ""Full Name"",
                             email      AS ""Email Address"",
                             phone      AS ""Phone Number"",
                             nid        AS ""National ID"",
                             balance    AS balance,
                             address    AS address,
                             join_date  AS ""Account Created""
                         FROM public.{tableName}";

            using (var connection = new NpgsqlConnection(DatabaseConnection.Connection))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        /// <summary>
        /// This declares from which table data will be read.
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
