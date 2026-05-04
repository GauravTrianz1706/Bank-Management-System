using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankDatabaseAccess
{
    public static class DatabaseConnection
    {
        // Fixed: Replaced Web.config/App.config connection string with environment variable
        // Use AWS Systems Manager Parameter Store or Secrets Manager for production
        public static readonly string Connection = 
            Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? 
            "Data Source=${DB_HOST};Initial Catalog=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD}";

       public enum Error
        {
            UsernameExist = 4001
        }

        public static int Execute(string query)
        {
            using (var connection = new SqlConnection(Connection))
            {
                try
                {
                    connection.Open();
                    return new SqlCommand(query, connection).ExecuteNonQuery();
                }
                catch (SqlException)
                {
                   return (int)Error.UsernameExist;
                }

            }
        }
    }
}
