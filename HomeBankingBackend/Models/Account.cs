namespace HomeBankingBackend.Models;

public class Account
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public decimal Balance { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
