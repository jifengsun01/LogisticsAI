using LogisticsAI.Api.Models;

namespace LogisticsAI.Api.Services;

public interface IShipmentService
{
    Task<IEnumerable<Shipment>> GetAllAsync();
    Task<Shipment?> GetByIdAsync(Guid id);
    Task<Shipment> CreateAsync(Shipment shipment);
    Task<Shipment?> UpdateStatusAsync(Guid id, ShipmentStatus status);
    Task<bool> DeleteAsync(Guid id);
}
