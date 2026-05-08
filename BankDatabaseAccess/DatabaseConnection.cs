using Microsoft.Data.SqlClient;

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
        public static string Connection { get; set; } = "Server=localhost;Database=OpenBankLocal;Integrated Security=true;TrustServerCertificate=true;";

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
