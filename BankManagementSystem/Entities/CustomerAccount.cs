namespace BankManagementSystem.Entities
{
    public class CustomerAccount
    {
        public int CustomerId { get; set; }

        public string AccountNumber { get; set; } = null!;

        public string OwnershipType { get; set; } = null!;

        public DateTime OwnershipStartDate { get; set; }

        public string AccountStatus { get; set; } = null!;

        public Customer Customer { get; set; } = null!;

        public Account Account { get; set; } = null!;
    }
}