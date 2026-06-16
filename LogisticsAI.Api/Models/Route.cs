namespace LogisticsAI.Api.Models;

public class Route
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ShipmentId { get; set; }
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public string TransportMode { get; set; } = string.Empty; // Air | Ground | Rail | Sea
    public DateTime EstimatedArrival { get; set; }
    public string Status { get; set; } = string.Empty;

    public Shipment Shipment { get; set; } = null!;
}
