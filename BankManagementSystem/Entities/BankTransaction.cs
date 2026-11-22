namespace BankManagementSystem.Entities
{
    public class BankTransaction
    {
        public int TransactionNumber { get; set; }

        public DateTime TransactionDate { get; set; }

        public decimal Amount { get; set; }

        public string Note { get; set; } = null!;

        public string TransactionType { get; set; } = null!;

        public string AccountNumber { get; set; } = null!;

        public Account Account { get; set; } = null!;
    }
}