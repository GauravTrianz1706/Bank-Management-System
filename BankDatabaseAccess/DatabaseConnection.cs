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
        // CONTAINERIZATION FIX: Replaced Web.config connection string 
        // (ConfigurationManager.ConnectionStrings["OpenBankLocal"].ConnectionString)
        // with environment variable for containerization
        // Blocker ID: blocker-12 (cz-dotnet-0055 - Web.config Transforms)
        // In production, use Azure App Configuration for environment-specific settings
        // and Azure Key Vault for secrets with Workload Identity for credential-free access
        // Environment variables to set:
        // - DB_CONNECTION_STRING: Full connection string, OR
        // - DB_HOST: Database server hostname
        // - DB_NAME: Database name
        // - DB_USER: Database username
        // - DB_PASSWORD: Database password (use Azure Key Vault in production)
        public static readonly string Connection = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
            ?? "Data Source=${DB_HOST};Initial Catalog=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};";

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
