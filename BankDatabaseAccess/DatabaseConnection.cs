using Npgsql;

namespace BankDatabaseAccess
{
    public enum Error
    {
        UsernameExist = -1,
        Success = 0
    }

    public static class DatabaseConnection
    {
        // Connection string should be configured via dependency injection or configuration in .NET 8
        // PostgreSQL connection string format with connection pooling
        public static string Connection { get; set; } = "Host=localhost;Database=openbanklocal;Username=postgres;Password=postgres;Port=5432;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100;Connection Lifetime=0;Timeout=30;";

        public static int Execute(string query)
        {
            using (var connection = new NpgsqlConnection(Connection))
            {
                try
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        return command.ExecuteNonQuery();
                    }
                }
                catch (NpgsqlException ex)
                {
                    // Check for unique constraint violation (duplicate key)
                    // PostgreSQL error code 23505 is for unique_violation
                    if (ex.SqlState == "23505")
                    {
                        return (int)Error.UsernameExist;
                    }
                    // Log the error or handle other PostgreSQL-specific errors
                    throw;
                }
            }
        }
    }
}
