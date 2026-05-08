namespace BankDatabaseAccess.EntityModel
{
    public  class PersonModel
    {
        /// <summary>
        /// This is the Person Username. Value can not be null.
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// This is the person Full name. Valu can not be null.
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// This is the Person Password. Value can not be null.
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// This is the person Email. Value can not be null.
        /// </summary>
        public string Eamil { get; set; } = string.Empty;
        /// <summary>
        /// This is person phone number
        /// </summary>
        public string Phone { get; set; } = string.Empty;
        /// <summary>
        /// This is person National ID number. Value can not be null.
        /// </summary>
        public string Nid { get; set; } = string.Empty;
        /// <summary>
        /// This is person Address. Value cant be null.
        /// </summary>
        public string Address { get; set; } = string.Empty;
    }
}
