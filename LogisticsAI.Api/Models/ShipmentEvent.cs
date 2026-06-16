namespace LogisticsAI.Api.Models;

public class ShipmentEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ShipmentId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Metadata { get; set; } // JSON string
    public DateTime OccurredAt { get; set; }

    public Shipment Shipment { get; set; } = null!;
}
