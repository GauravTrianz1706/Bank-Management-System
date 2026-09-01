using System;
using System.Data.SqlClient;

namespace BankDatabaseAccess
{
    public static class DatabaseConnection
    {
        // Replaced Web.config/ConfigurationManager with environment variable for cloud-native configuration.
        // The connection string is now read from the BANKAPP_CONNECTION_STRING environment variable,
        // enabling runtime configuration without rebuilding (12-factor app principle).
        public static readonly string Connection =
            Environment.GetEnvironmentVariable("BANKAPP_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Database connection string is not configured. " +
                "Set the 'BANKAPP_CONNECTION_STRING' environment variable.");

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
