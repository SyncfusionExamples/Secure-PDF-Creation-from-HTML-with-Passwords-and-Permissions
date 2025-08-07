namespace SecurePdfGeneration
{
    public class StatementBuilder
    {
        public BankDetails Bank { get; set; }
        public AccountStatement Statement { get; set; }
        public List<TransactionDetail> Transactions { get; set; }
    }
}
