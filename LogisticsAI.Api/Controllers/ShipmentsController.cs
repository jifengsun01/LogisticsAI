using LogisticsAI.Api.Data;
using LogisticsAI.Api.Models;
using LogisticsAI.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController(AppDbContext db, IShipmentService shipmentService, IAiInsightService ai) : ControllerBase
{
    // GET /api/shipments?status=Critical&carrier=FedEx&limit=20
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? carrier,
        [FromQuery] int limit = 20)
    {
        var query = db.Shipments.AsQueryable();

        if (!string.IsNullOrEmpty(status) &&
            Enum.TryParse<ShipmentStatus>(status, ignoreCase: true, out var statusEnum))
            query = query.Where(s => s.Status == statusEnum);

        if (!string.IsNullOrEmpty(carrier))
            query = query.Where(s => s.Carrier.ToLower() == carrier.ToLower());

        var results = await query
            .OrderByDescending(s => s.DelayHours)
            .Take(limit)
            .ToListAsync();

        return Ok(results);
    }

    // GET /api/shipments/kpis
    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis()
    {
        var all = await db.Shipments.ToListAsync();

        var totalActive = all.Count(s =>
            s.Status is ShipmentStatus.InTransit or ShipmentStatus.Delayed or ShipmentStatus.Critical);

        var delivered = all.Where(s => s.Status == ShipmentStatus.Delivered).ToList();
        var onTimeRate = delivered.Count == 0
            ? 1m
            : (decimal)delivered.Count(s => s.DelayHours == 0) / delivered.Count;

        var delayed = all.Where(s => s.DelayHours > 0).ToList();
        var avgDelayHours = delayed.Count == 0 ? 0m : delayed.Average(s => s.DelayHours);

        var criticalCount = all.Count(s => s.Status == ShipmentStatus.Critical);

        return Ok(new
        {
            totalActive,
            onTimeRate = Math.Round(onTimeRate, 4),
            avgDelayHours = Math.Round(avgDelayHours, 2),
            criticalCount
        });
    }

    // GET /api/shipments/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var shipment = await db.Shipments
            .Include(s => s.Events.OrderBy(e => e.OccurredAt))
            .Include(s => s.RcaResults.OrderByDescending(r => r.AnalyzedAt))
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shipment is null) return NotFound();

        return Ok(new
        {
            shipment.Id,
            shipment.TrackingNumber,
            shipment.Carrier,
            shipment.OriginCity,
            shipment.DestinationCity,
            Status = shipment.Status.ToString(),
            shipment.WeightKg,
            shipment.ScheduledEta,
            shipment.ActualEta,
            shipment.DelayHours,
            shipment.CreatedAt,
            Events = shipment.Events.Select(e => new
            {
                e.EventType, e.Location, e.Description, e.OccurredAt, e.Metadata
            }),
            LatestRca = shipment.RcaResults.FirstOrDefault() is RcaResult rca
                ? new { rca.RootCauseCategory, rca.AiSummary, rca.ConfidenceScore, rca.AnalyzedAt }
                : null
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Shipment shipment)
    {
        shipment.Id = Guid.NewGuid();
        shipment.TrackingNumber = $"LOG-{shipment.Id.ToString()[..8].ToUpper()}";
        var insight = await ai.GetShipmentInsightAsync(
            shipment.OriginCity, shipment.DestinationCity, shipment.WeightKg);
        shipment.RcaResults.Add(new RcaResult
        {
            RootCauseCategory = "AI_INSIGHT",
            AiSummary = insight,
            ConfidenceScore = 0,
            AnalyzedAt = DateTime.UtcNow
        });
        var created = await shipmentService.CreateAsync(shipment);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ShipmentStatus status)
    {
        var updated = await shipmentService.UpdateStatusAsync(id, status);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await shipmentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
