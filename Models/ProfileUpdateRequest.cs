namespace brokenaccesscontrol.Models;

// A01 - Broken Access Control (mass assignment / overposting).
// Model exposto direto no bind: cliente consegue setar Role/DailyLimit/IsAdmin.
public class ProfileUpdateRequest
{
    public string? Name { get; set; }
    public string? Cpf { get; set; }
    public string? Role { get; set; }
    public decimal? DailyLimit { get; set; }
    public bool? IsAdmin { get; set; }
}
