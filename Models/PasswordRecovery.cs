namespace brokenaccesscontrol.Models;

public class PasswordRecovery
{
    public string? Login { get; set; }
}

// Reset de senha usando token previsível (A04) - sem verificar identidade (A06)
public class PasswordReset
{
    public string? Login { get; set; }
    public string? Token { get; set; }
    public string? NewPassword { get; set; }
}
