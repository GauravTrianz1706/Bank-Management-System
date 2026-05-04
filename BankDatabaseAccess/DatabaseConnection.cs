using System;
using System.Data.SqlClient;

namespace BankDatabaseAccess
{
    public static class DatabaseConnection
    {
        // Fixed: Replaced Web.config ConfigurationManager with environment variables
        // Connection string should be stored in AWS Systems Manager Parameter Store or Secrets Manager
        public static readonly string Connection = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
            ?? "Server=${DB_HOST};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD}";

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
