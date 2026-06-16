using LogisticsAI.Api.Data;
using LogisticsAI.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAI.Api.Services;

public class ShipmentService(AppDbContext db) : IShipmentService
{
    public async Task<IEnumerable<Shipment>> GetAllAsync() =>
        await db.Shipments.OrderByDescending(s => s.CreatedAt).ToListAsync();

    public async Task<Shipment?> GetByIdAsync(Guid id) =>
        await db.Shipments.FindAsync(id);

    public async Task<Shipment> CreateAsync(Shipment shipment)
    {
        db.Shipments.Add(shipment);
        await db.SaveChangesAsync();
        return shipment;
    }

    public async Task<Shipment?> UpdateStatusAsync(Guid id, ShipmentStatus status)
    {
        var shipment = await db.Shipments.FindAsync(id);
        if (shipment is null) return null;
        shipment.Status = status;
        await db.SaveChangesAsync();
        return shipment;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var shipment = await db.Shipments.FindAsync(id);
        if (shipment is null) return false;
        db.Shipments.Remove(shipment);
        await db.SaveChangesAsync();
        return true;
    }
}
