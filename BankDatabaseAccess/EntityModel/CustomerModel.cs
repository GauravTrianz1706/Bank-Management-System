namespace BankDatabaseAccess.EntityModel
{
    public class CustomerModel : PersonModel
    {
        /// <summary>
        /// This is Customer's Balance. Set is only use for Deposit perpose.
        /// </summary>
        public decimal Balance { get; set; }
    }
}
