namespace HomeBankingBackend.Models;

public class Transaction
{
    public int Id { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    
    public int? SourceAccountId { get; set; }
    public Account? SourceAccount { get; set; }
    
    public int? DestinationAccountId { get; set; }
    public Account? DestinationAccount { get; set; }
}
