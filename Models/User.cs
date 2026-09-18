namespace brokenaccesscontrol.Models;

public class User
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Cpf { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public string? MfaSecret { get; set; }
    public decimal DailyLimit { get; set; }
    public string? ResetToken { get; set; }
    public DateTime DateInsert { get; set; }
    public DateTime? DateUpdate { get; set; }
    public bool IsAdmin { get; set; }
    public bool Inativo { get; set; }
    public DateTime? dateChangePassword { get; set; }
}
