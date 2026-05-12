using System;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace BankDatabaseAccess
{
    public static class DatabaseConnection
    {
        public static readonly string Connection = ConfigurationManager.ConnectionStrings["OpenBankLocal"]?.ConnectionString ?? "Data Source=.;Initial Catalog=OpenBankLocal;Integrated Security=True;TrustServerCertificate=True";

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
