using Microsoft.Data.SqlClient;

namespace BankDatabaseAccess
{
    public static class DatabaseConnection
    {
        public static string Connection { get; set; } = string.Empty;

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
