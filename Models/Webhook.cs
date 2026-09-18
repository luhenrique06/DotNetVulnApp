namespace brokenaccesscontrol.Models;

public class Webhook
{
    public int Id { get; set; }
    public string? OwnerId { get; set; }
    public string? CallbackUrl { get; set; }
    public string? Secret { get; set; }
}

// Payload recebido no callback de PIX (A08 - sem verificação de assinatura)
public class WebhookCallback
{
    public int ToAccount { get; set; }
    public decimal Amount { get; set; }
    public string? ExternalId { get; set; }
    public string? Signature { get; set; }
}

// Registro de webhook -> gatilho de teste (A-SSRF via callbackUrl)
public class WebhookRegisterRequest
{
    public string? CallbackUrl { get; set; }
    public string? Secret { get; set; }
}
