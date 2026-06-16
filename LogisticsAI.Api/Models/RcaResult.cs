namespace LogisticsAI.Api.Models;

public class RcaResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ShipmentId { get; set; }
    public string RootCauseCategory { get; set; } = string.Empty;
    public string AiSummary { get; set; } = string.Empty;
    public string? ContributingFactors { get; set; } // JSON string
    public decimal ConfidenceScore { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    public Shipment Shipment { get; set; } = null!;
}
