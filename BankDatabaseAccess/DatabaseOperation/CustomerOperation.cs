using BankDatabaseAccess.EntityModel;
using Npgsql;

namespace BankDatabaseAccess.DatabaseOperation
{
    public class CustomerOperation : IOperations, ITransaction
    {
        private const decimal InitialBalance = 100;

        /// <summary>
        /// This method is use for Customers Registration Purpose.
        /// Uses parameterized queries to prevent SQL injection.
        /// </summary>
        /// <param name="personModel">Take a Customer Object</param>
        /// <returns>Return Row Number</returns>
        public int Insert(PersonModel personModel)
        {
            using (var connection = new NpgsqlConnection(DatabaseConnection.Connection))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO public.customers(username, full_name, password, email, phone, nid, address, balance)
                      VALUES (@username, @full_name, @password, @email, @phone, @nid, @address, @balance)", connection))
                {
                    cmd.Parameters.AddWithValue("@username", personModel.Username ?? string.Empty);
                    cmd.Parameters.AddWithValue("@full_name", personModel.FullName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@password", personModel.Password ?? string.Empty);
                    cmd.Parameters.AddWithValue("@email", personModel.Eamil ?? string.Empty);
                    cmd.Parameters.AddWithValue("@phone", personModel.Phone ?? string.Empty);
                    cmd.Parameters.AddWithValue("@nid", personModel.Nid ?? string.Empty);
                    cmd.Parameters.AddWithValue("@address", personModel.Address ?? string.Empty);
                    cmd.Parameters.AddWithValue("@balance", InitialBalance);
                    try
                    {
                        return cmd.ExecuteNonQuery();
                    }
                    catch (NpgsqlException)
                    {
                        return (int)DatabaseConnection.Error.UsernameExist;
                    }
                }
            }
        }

        /// <summary>
        /// Delete a user from data base
        /// </summary>
        /// <param name="personModel">Take a Customer Object</param>
        /// <returns>row effect</returns>
        public int Delete(PersonModel personModel)
        {
            return new EmployeeOperations().Delete(personModel);
        }

        /// <summary>
        /// Update a user form data base using parameterized queries.
        /// </summary>
        /// <param name="personModel">Take a Customer Object</param>
        /// <returns>row effect</returns>
        public int Update(PersonModel personModel)
        {
            using (var connection = new NpgsqlConnection(DatabaseConnection.Connection))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE public.customers SET
                         email   = @email,
                         phone   = @phone,
                         nid     = @nid,
                         address = @address
                      WHERE username = @username", connection))
                {
                    cmd.Parameters.AddWithValue("@email", personModel.Eamil ?? string.Empty);
                    cmd.Parameters.AddWithValue("@phone", personModel.Phone ?? string.Empty);
                    cmd.Parameters.AddWithValue("@nid", personModel.Nid ?? string.Empty);
                    cmd.Parameters.AddWithValue("@address", personModel.Address ?? string.Empty);
                    cmd.Parameters.AddWithValue("@username", personModel.Username ?? string.Empty);
                    try
                    {
                        return cmd.ExecuteNonQuery();
                    }
                    catch (NpgsqlException)
                    {
                        return (int)DatabaseConnection.Error.UsernameExist;
                    }
                }
            }
        }

        /// <summary>
        /// Update customer balance using parameterized queries.
        /// </summary>
        /// <param name="personModel">Take a Customer Object</param>
        /// <param name="amount"></param>
        /// <returns>row effected</returns>
        public int UpdateBalance(PersonModel personModel, decimal amount)
        {
            using (var connection = new NpgsqlConnection(DatabaseConnection.Connection))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE public.customers SET balance = @balance WHERE username = @username", connection))
                {
                    cmd.Parameters.AddWithValue("@balance", amount);
                    cmd.Parameters.AddWithValue("@username", personModel.Username ?? string.Empty);
                    try
                    {
                        return cmd.ExecuteNonQuery();
                    }
                    catch (NpgsqlException)
                    {
                        return (int)DatabaseConnection.Error.UsernameExist;
                    }
                }
            }
        }
    }
}
