namespace LogisticsAI.Api.Models;

public class Shipment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TrackingNumber { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; } = ShipmentStatus.InTransit;
    public decimal WeightKg { get; set; }
    public DateTime ScheduledEta { get; set; }
    public DateTime? ActualEta { get; set; }
    public decimal DelayHours { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Route> Routes { get; set; } = [];
    public ICollection<ShipmentEvent> Events { get; set; } = [];
    public ICollection<RcaResult> RcaResults { get; set; } = [];
}
