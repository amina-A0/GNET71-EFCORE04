namespace BankManagementSystem.Entities
{
    public class Account
    {
        public string AccountNumber { get; set; } = null!;

        public decimal CurrentBalance { get; set; }

        public string AccountType { get; set; } = null!;

        public DateTime OpeningDate { get; set; }

        public string BranchCode { get; set; } = null!;

        public Branch Branch { get; set; } = null!;

        public ICollection<CustomerAccount> CustomerAccounts { get; set; }
            = new List<CustomerAccount>();

        public ICollection<BankTransaction> Transactions { get; set; }
            = new List<BankTransaction>();
    }
}