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
        // Blocker-17 (cr-dotnet-0010): Replaced Web.config/ConfigurationManager connection string
        // with environment variable configuration for GKE/cloud-native deployment.
        // GKE injects environment-specific configuration via Kubernetes ConfigMaps and environment variables.
        public static readonly string Connection =
            Environment.GetEnvironmentVariable("OPENBANK_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Database connection string not configured. " +
                "Set the OPENBANK_CONNECTION_STRING environment variable.");

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
