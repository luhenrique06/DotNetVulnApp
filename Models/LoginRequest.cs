namespace brokenaccesscontrol.Models;

public class LoginRequest
{
    public string? Login { get; set; }
    public string? Password { get; set; }
    public bool? IsAdmin { get; set; }        // A01: mass-assignment de privilégio
    public string? MfaCode { get; set; }
    public bool? MfaVerified { get; set; }     // A07: confiança client-side no MFA
}
