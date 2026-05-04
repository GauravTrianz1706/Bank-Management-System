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
        // Blocker 18 Fix: Replace Web.config with environment variables for cloud-native configuration
        // Blocker 15 Fix: Use connection string that supports RDS Proxy and connection pooling
        public static readonly string Connection = 
            Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") 
            ?? "Server=localhost;Database=BankDB;Integrated Security=true;";

       public enum Error
        {
            UsernameExist = 4001
        }

        // Blocker 15 Fix: Refactored to support RDS Proxy pattern with proper connection pooling
        // Connection pooling is now handled by ADO.NET connection pool and RDS Proxy at infrastructure level
        public static int Execute(string query)
        {
            // Using statement ensures proper connection disposal and returns connection to pool
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
