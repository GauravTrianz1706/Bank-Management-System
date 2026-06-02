using BankDatabaseAccess.EntityModel;

namespace BankDatabaseAccess.DatabaseOperation
{
    public class EmployeeOperations : IOperations
    {
        /// <summary>
        /// This method is use for Employee Registration purpose 
        /// </summary>
        /// <param name="personModel">Take an Employee Object</param>
        /// <returns>Returns Row Number</returns>
        public int Insert(PersonModel personModel)
        {
            var query = @"INSERT INTO public.employee(username, full_name, password, email, address, phone, nid, salary) 
                          VALUES ('" + personModel.Username + "'," +
                          "'" + personModel.FullName + "'," +
                          "'" + personModel.Password + "'," +
                          "'" + personModel.Eamil + "'," +
                          "'" + personModel.Address + "'," +
                          "'" + personModel.Phone + "'," +
                          "'" + personModel.Nid + "'," +
                          "'" + new Random().Next(30000, 1000000).ToString() + "')"; // generates Salary from 30k to 100k
            return DatabaseConnection.Execute(query);
        }

        /// <summary>
        /// This method is use for Deleting an existing Customer account from dashboard
        /// </summary>
        /// <param name="personModel">Takes a Customer Object</param>
        /// <returns>Returns Row Number</returns>
        public int Delete(PersonModel personModel)
        {
            var query = @"DELETE FROM public.customers 
                        WHERE username = '" + personModel.Username + "'";
            return DatabaseConnection.Execute(query);
        }

        /// <summary>
        /// This method is use for updating an existing Customer Account from dashboard
        /// </summary>
        /// <param name="personModel">Takes a Customer Object</param>
        /// <returns>Returns Row Number</returns>
        public int Update(PersonModel personModel)
        {
            var query = @"UPDATE public.customers SET 
                        email = '" + personModel.Eamil + "'," +
                        "phone = '" + personModel.Phone + "'," +
                        "address = '" + personModel.Address + "'," +
                        "nid = '" + personModel.Nid + "' " +
                        "WHERE username = '" + personModel.Username + "'";

            return DatabaseConnection.Execute(query);
        }

        /// <summary>
        /// Update Employee's data from db
        /// </summary>
        /// <param name="personModel">takes an employee object</param>
        /// <returns></returns>
        public int SelfUpdate(PersonModel personModel)
        {
            var query = @"UPDATE public.employee SET 
                        email = '" + personModel.Eamil + "'," +
                        "phone = '" + personModel.Phone + "'," +
                        "address = '" + personModel.Address + "'," +
                        "nid = '" + personModel.Nid + "' " +
                        "WHERE username = '" + personModel.Username + "'";
            return DatabaseConnection.Execute(query);
        }
    }
}
