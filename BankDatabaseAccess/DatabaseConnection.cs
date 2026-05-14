using Npgsql;
using Microsoft.Extensions.Configuration;

namespace BankDatabaseAccess
{
    public static class DatabaseConnection
    {
        private static readonly IConfiguration _configuration;

        static DatabaseConnection()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }

        public static readonly string Connection = _configuration.GetConnectionString("OpenBankLocal") ?? string.Empty;

        public enum Error
        {
            UsernameExist = 4001
        }

        public static int Execute(string query)
        {
            using (var connection = new NpgsqlConnection(Connection))
            {
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
}
