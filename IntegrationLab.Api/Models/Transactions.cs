namespace IntegrationLab.Api.Models;

public class Transaction
{
    public int Id { get; set; }

    public string ProviderTransactionId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "ISK";

    public string PaymentStatus { get; set; } = "Approved";

    public string ProcessingStatus { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
