using BankDatabaseAccess.EntityModel;
using Npgsql;

namespace BankDatabaseAccess.DatabaseOperation
{
    public class EmployeeOperations : IOperations
    {
        /// <summary>
        /// This method is use for Employee Registration purpose.
        /// Uses parameterized queries to prevent SQL injection.
        /// </summary>
        /// <param name="personModel">Take an Employee Object</param>
        /// <returns>Returns Row Number</returns>
        public int Insert(PersonModel personModel)
        {
            using (var connection = new NpgsqlConnection(DatabaseConnection.Connection))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO public.employee(username, full_name, password, email, address, phone, nid, salary)
                      VALUES (@username, @full_name, @password, @email, @address, @phone, @nid, @salary)", connection))
                {
                    cmd.Parameters.AddWithValue("@username", personModel.Username ?? string.Empty);
                    cmd.Parameters.AddWithValue("@full_name", personModel.FullName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@password", personModel.Password ?? string.Empty);
                    cmd.Parameters.AddWithValue("@email", personModel.Eamil ?? string.Empty);
                    cmd.Parameters.AddWithValue("@address", personModel.Address ?? string.Empty);
                    cmd.Parameters.AddWithValue("@phone", personModel.Phone ?? string.Empty);
                    cmd.Parameters.AddWithValue("@nid", personModel.Nid ?? string.Empty);
                    // generates Salary from 30k to 1000k
                    cmd.Parameters.AddWithValue("@salary", new Random().Next(30000, 1000000).ToString());
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
        /// This method is use for Deleting an existing Customer account form dashboard.
        /// Uses parameterized queries to prevent SQL injection.
        /// </summary>
        /// <param name="personModel">Takes an Customer Object</param>
        /// <returns>Returns Row Number</returns>
        public int Delete(PersonModel personModel)
        {
            using (var connection = new NpgsqlConnection(DatabaseConnection.Connection))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(
                    @"DELETE FROM public.customers WHERE username = @username", connection))
                {
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
        /// This method is use for update an existing Customer Account from dashboard.
        /// Uses parameterized queries to prevent SQL injection.
        /// </summary>
        /// <param name="personModel">Takes an Customer Object</param>
        /// <returns>Returns Row Number</returns>
        public int Update(PersonModel personModel)
        {
            using (var connection = new NpgsqlConnection(DatabaseConnection.Connection))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE public.customers SET
                         email   = @email,
                         phone   = @phone,
                         address = @address,
                         nid     = @nid
                      WHERE username = @username", connection))
                {
                    cmd.Parameters.AddWithValue("@email", personModel.Eamil ?? string.Empty);
                    cmd.Parameters.AddWithValue("@phone", personModel.Phone ?? string.Empty);
                    cmd.Parameters.AddWithValue("@address", personModel.Address ?? string.Empty);
                    cmd.Parameters.AddWithValue("@nid", personModel.Nid ?? string.Empty);
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
        /// Update Employee's data from db.
        /// Uses parameterized queries to prevent SQL injection.
        /// </summary>
        /// <param name="personModel">takes an employee object</param>
        /// <returns></returns>
        public int SelfUpdate(PersonModel personModel)
        {
            using (var connection = new NpgsqlConnection(DatabaseConnection.Connection))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE public.employee SET
                         email   = @email,
                         phone   = @phone,
                         address = @address,
                         nid     = @nid
                      WHERE username = @username", connection))
                {
                    cmd.Parameters.AddWithValue("@email", personModel.Eamil ?? string.Empty);
                    cmd.Parameters.AddWithValue("@phone", personModel.Phone ?? string.Empty);
                    cmd.Parameters.AddWithValue("@address", personModel.Address ?? string.Empty);
                    cmd.Parameters.AddWithValue("@nid", personModel.Nid ?? string.Empty);
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
