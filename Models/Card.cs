namespace brokenaccesscontrol.Models;

public class Card
{
    public int Id { get; set; }
    public string? OwnerId { get; set; }
    public string? Pan { get; set; }
    public string? Cvv { get; set; }
    public string? Expiry { get; set; }
    public decimal CardLimit { get; set; }
}
