namespace brokenaccesscontrol.Models;

public class Account
{
    public int AccountId { get; set; }
    public string? Agencia { get; set; }
    public string? Conta { get; set; }
    public string? Cpf { get; set; }
    public string? OwnerId { get; set; }
    public decimal Saldo { get; set; }
    public string? Status { get; set; }
}
