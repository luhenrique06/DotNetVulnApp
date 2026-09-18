namespace brokenaccesscontrol.Models;

public class Transfer
{
    public int Id { get; set; }
    public int FromAccount { get; set; }
    public int ToAccount { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public string? CreatedAt { get; set; }
    public string? IdempotencyKey { get; set; }
}

public class TransferRequest
{
    public int FromAccount { get; set; }
    public int ToAccount { get; set; }
    public decimal Amount { get; set; }
    public string? IdempotencyKey { get; set; }
}
