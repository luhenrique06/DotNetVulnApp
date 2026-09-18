namespace brokenaccesscontrol.Models;

public class Coupon
{
    public string? Code { get; set; }
    public int ValuePct { get; set; }
    public int UsesLeft { get; set; }
}

public class CouponApplyRequest
{
    public string? Code { get; set; }
    public int AccountId { get; set; }
}
