using Npgsql;
using Microsoft.Extensions.Configuration;

namespace BankDatabaseAccess
{
    public static class DatabaseConnection
    {
        private static IConfiguration? _configuration;

        public static void Configure(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string Connection
        {
            get
            {
                if (_configuration != null)
                {
                    return _configuration.GetConnectionString("OpenBankLocal")
                        ?? throw new InvalidOperationException("Connection string 'OpenBankLocal' not found.");
                }
                // Fallback: read from environment variable for .NET 8 compatibility
                return Environment.GetEnvironmentVariable("OpenBankLocal_ConnectionString")
                    ?? throw new InvalidOperationException("Database connection string is not configured. Call DatabaseConnection.Configure(IConfiguration) at startup.");
            }
        }

        public enum Error
        {
            UsernameExist = 4001
        }

        public static int Execute(string query)
        {
            using var connection = new NpgsqlConnection(Connection);
            try
            {
                connection.Open();
                return new NpgsqlCommand(query, connection).ExecuteNonQuery();
            }
            catch (NpgsqlException)
            {
                return (int)Error.UsernameExist;
            }
        }
    }
}
