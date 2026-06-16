using LogisticsAI.Api.Models;

namespace LogisticsAI.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Shipments.Any()) return;

        var now = DateTime.UtcNow;

        // ── Shipments ──────────────────────────────────────────────────────────
        var shipments = new List<Shipment>
        {
            // 3 Critical (delay > 4h)
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"),
                TrackingNumber = "FDX-2024-001", Carrier = "FedEx",
                OriginCity = "Los Angeles, CA", DestinationCity = "New York, NY",
                Status = ShipmentStatus.Critical, WeightKg = 45.5m,
                ScheduledEta = now.AddHours(-6), ActualEta = null,
                DelayHours = 7.5m, CreatedAt = now.AddDays(-3)
            },
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000002"),
                TrackingNumber = "UPS-2024-002", Carrier = "UPS",
                OriginCity = "Chicago, IL", DestinationCity = "Miami, FL",
                Status = ShipmentStatus.Critical, WeightKg = 120.0m,
                ScheduledEta = now.AddHours(-8), ActualEta = null,
                DelayHours = 10.2m, CreatedAt = now.AddDays(-4)
            },
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000003"),
                TrackingNumber = "DHL-2024-003", Carrier = "DHL",
                OriginCity = "Seattle, WA", DestinationCity = "Houston, TX",
                Status = ShipmentStatus.Critical, WeightKg = 88.3m,
                ScheduledEta = now.AddHours(-5), ActualEta = null,
                DelayHours = 6.0m, CreatedAt = now.AddDays(-2)
            },

            // 4 Delayed (1-4h)
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000004"),
                TrackingNumber = "FDX-2024-004", Carrier = "FedEx",
                OriginCity = "Phoenix, AZ", DestinationCity = "Denver, CO",
                Status = ShipmentStatus.Delayed, WeightKg = 22.1m,
                ScheduledEta = now.AddHours(-2), ActualEta = null,
                DelayHours = 2.5m, CreatedAt = now.AddDays(-1)
            },
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000005"),
                TrackingNumber = "UPS-2024-005", Carrier = "UPS",
                OriginCity = "Atlanta, GA", DestinationCity = "Boston, MA",
                Status = ShipmentStatus.Delayed, WeightKg = 55.0m,
                ScheduledEta = now.AddHours(-1), ActualEta = null,
                DelayHours = 1.8m, CreatedAt = now.AddDays(-2)
            },
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000006"),
                TrackingNumber = "USPS-2024-006", Carrier = "USPS",
                OriginCity = "Dallas, TX", DestinationCity = "San Francisco, CA",
                Status = ShipmentStatus.Delayed, WeightKg = 8.7m,
                ScheduledEta = now.AddHours(-3), ActualEta = null,
                DelayHours = 3.2m, CreatedAt = now.AddDays(-1)
            },
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000007"),
                TrackingNumber = "DHL-2024-007", Carrier = "DHL",
                OriginCity = "Portland, OR", DestinationCity = "Las Vegas, NV",
                Status = ShipmentStatus.Delayed, WeightKg = 34.4m,
                ScheduledEta = now.AddHours(-2), ActualEta = null,
                DelayHours = 2.1m, CreatedAt = now.AddDays(-1)
            },

            // 5 InTransit
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000008"),
                TrackingNumber = "FDX-2024-008", Carrier = "FedEx",
                OriginCity = "Minneapolis, MN", DestinationCity = "Nashville, TN",
                Status = ShipmentStatus.InTransit, WeightKg = 67.9m,
                ScheduledEta = now.AddHours(12), ActualEta = null,
                DelayHours = 0, CreatedAt = now.AddDays(-1)
            },
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000009"),
                TrackingNumber = "UPS-2024-009", Carrier = "UPS",
                OriginCity = "Detroit, MI", DestinationCity = "Charlotte, NC",
                Status = ShipmentStatus.InTransit, WeightKg = 15.2m,
                ScheduledEta = now.AddHours(8), ActualEta = null,
                DelayHours = 0, CreatedAt = now.AddHours(-18)
            },
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000010"),
                TrackingNumber = "DHL-2024-010", Carrier = "DHL",
                OriginCity = "Kansas City, MO", DestinationCity = "Philadelphia, PA",
                Status = ShipmentStatus.InTransit, WeightKg = 42.6m,
                ScheduledEta = now.AddHours(20), ActualEta = null,
                DelayHours = 0, CreatedAt = now.AddDays(-1)
            },
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000011"),
                TrackingNumber = "USPS-2024-011", Carrier = "USPS",
                OriginCity = "Salt Lake City, UT", DestinationCity = "Cleveland, OH",
                Status = ShipmentStatus.InTransit, WeightKg = 5.4m,
                ScheduledEta = now.AddHours(36), ActualEta = null,
                DelayHours = 0, CreatedAt = now.AddHours(-12)
            },
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000012"),
                TrackingNumber = "FDX-2024-012", Carrier = "FedEx",
                OriginCity = "San Diego, CA", DestinationCity = "Austin, TX",
                Status = ShipmentStatus.InTransit, WeightKg = 91.0m,
                ScheduledEta = now.AddHours(24), ActualEta = null,
                DelayHours = 0, CreatedAt = now.AddDays(-1)
            },

            // 3 Delivered
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000013"),
                TrackingNumber = "UPS-2024-013", Carrier = "UPS",
                OriginCity = "New York, NY", DestinationCity = "Chicago, IL",
                Status = ShipmentStatus.Delivered, WeightKg = 28.0m,
                ScheduledEta = now.AddDays(-2), ActualEta = now.AddDays(-2).AddHours(1),
                DelayHours = 1.0m, CreatedAt = now.AddDays(-5)
            },
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000014"),
                TrackingNumber = "DHL-2024-014", Carrier = "DHL",
                OriginCity = "Boston, MA", DestinationCity = "Seattle, WA",
                Status = ShipmentStatus.Delivered, WeightKg = 73.5m,
                ScheduledEta = now.AddDays(-3), ActualEta = now.AddDays(-3),
                DelayHours = 0, CreatedAt = now.AddDays(-7)
            },
            new() {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000015"),
                TrackingNumber = "USPS-2024-015", Carrier = "USPS",
                OriginCity = "Miami, FL", DestinationCity = "Denver, CO",
                Status = ShipmentStatus.Delivered, WeightKg = 3.2m,
                ScheduledEta = now.AddDays(-1), ActualEta = now.AddDays(-1),
                DelayHours = 0, CreatedAt = now.AddDays(-4)
            },
        };

        db.Shipments.AddRange(shipments);

        // ── Routes ─────────────────────────────────────────────────────────────
        var routes = new List<Models.Route>
        {
            new() { ShipmentId = shipments[0].Id, FromLocation = "Los Angeles, CA", ToLocation = "Phoenix, AZ",    SequenceOrder = 1, TransportMode = "Ground", EstimatedArrival = shipments[0].ScheduledEta.AddHours(-20), Status = "Completed" },
            new() { ShipmentId = shipments[0].Id, FromLocation = "Phoenix, AZ",    ToLocation = "New York, NY",    SequenceOrder = 2, TransportMode = "Air",    EstimatedArrival = shipments[0].ScheduledEta,               Status = "Delayed"   },

            new() { ShipmentId = shipments[1].Id, FromLocation = "Chicago, IL",    ToLocation = "Indianapolis, IN", SequenceOrder = 1, TransportMode = "Ground", EstimatedArrival = shipments[1].ScheduledEta.AddHours(-15), Status = "Completed" },
            new() { ShipmentId = shipments[1].Id, FromLocation = "Indianapolis, IN", ToLocation = "Miami, FL",     SequenceOrder = 2, TransportMode = "Ground", EstimatedArrival = shipments[1].ScheduledEta,                Status = "Delayed"   },

            new() { ShipmentId = shipments[7].Id, FromLocation = "Minneapolis, MN", ToLocation = "Chicago, IL",   SequenceOrder = 1, TransportMode = "Ground", EstimatedArrival = now.AddHours(4),  Status = "InTransit" },
            new() { ShipmentId = shipments[7].Id, FromLocation = "Chicago, IL",     ToLocation = "Nashville, TN", SequenceOrder = 2, TransportMode = "Ground", EstimatedArrival = now.AddHours(12), Status = "Pending"   },

            new() { ShipmentId = shipments[12].Id, FromLocation = "New York, NY", ToLocation = "Chicago, IL",     SequenceOrder = 1, TransportMode = "Air",    EstimatedArrival = shipments[12].ActualEta!.Value.AddHours(-2), Status = "Completed" },
        };

        db.Routes.AddRange(routes);

        // ── ShipmentEvents ─────────────────────────────────────────────────────
        var events = new List<ShipmentEvent>();

        void AddEvents(Guid shipmentId, DateTime baseTime, params (string type, string loc, string desc, string? meta)[] evts)
        {
            foreach (var (type, loc, desc, meta) in evts)
                events.Add(new ShipmentEvent { ShipmentId = shipmentId, EventType = type, Location = loc, Description = desc, Metadata = meta, OccurredAt = baseTime });
        }

        // Critical shipments
        AddEvents(shipments[0].Id, now.AddDays(-3),
            ("departed",      "Los Angeles, CA",  "Shipment picked up by FedEx",            null),
            ("arrived_hub",   "Phoenix, AZ",      "Arrived at Phoenix sorting hub",          """{"hub_code":"PHX01"}"""),
            ("weather_hold",  "Phoenix, AZ",      "Hold due to severe weather in northeast", """{"severity":"high","estimated_delay_hours":7.5}"""));

        AddEvents(shipments[1].Id, now.AddDays(-4),
            ("departed",     "Chicago, IL",       "Package picked up at shipper facility",   null),
            ("arrived_hub",  "Indianapolis, IN",  "Arrived at UPS hub",                      """{"hub_code":"IND02"}"""),
            ("customs_hold", "Indianapolis, IN",  "Regulatory inspection delay",             """{"reason":"random_inspection","ticket":"INS-8821"}"""));

        AddEvents(shipments[2].Id, now.AddDays(-2),
            ("departed",    "Seattle, WA",        "DHL collected shipment",                  null),
            ("weather_hold","Portland, OR",       "Storm system causing ground stop",         """{"severity":"medium","storm_id":"WX-2024-443"}"""),
            ("in_transit",  "Sacramento, CA",     "Resumed transit after weather cleared",   null));

        // Delayed shipments
        AddEvents(shipments[3].Id, now.AddDays(-1),
            ("departed",   "Phoenix, AZ",         "FedEx pickup complete",                   null),
            ("arrived_hub","Albuquerque, NM",      "Arrived at Albuquerque hub",              """{"hub_code":"ABQ01"}"""),
            ("mechanical", "Albuquerque, NM",      "Vehicle mechanical issue – rescheduled", """{"vehicle_id":"NM-FDX-442","delay_hours":2.5}"""));

        AddEvents(shipments[4].Id, now.AddDays(-2),
            ("departed",   "Atlanta, GA",          "UPS collected",                           null),
            ("traffic_delay","Charlotte, NC",      "Highway closure reroute",                 """{"highway":"I-85","delay_minutes":108}"""));

        AddEvents(shipments[5].Id, now.AddDays(-1),
            ("departed",   "Dallas, TX",           "USPS origin scan",                        null),
            ("arrived_hub","Albuquerque, NM",       "USPS distribution center",               null),
            ("weather_hold","Amarillo, TX",         "High-wind advisory delay",               """{"wind_mph":65,"advisory":"TX-WND-2024"}"""));

        AddEvents(shipments[6].Id, now.AddDays(-1),
            ("departed",    "Portland, OR",         "DHL pickup",                              null),
            ("arrived_hub", "Sacramento, CA",       "Hub scan",                                null),
            ("traffic_delay","Sacramento, CA",      "Freeway incident causing delay",          """{"incident_id":"CA-5-882","delay_minutes":126}"""));

        // InTransit shipments
        AddEvents(shipments[7].Id, now.AddDays(-1),
            ("departed",   "Minneapolis, MN",       "FedEx collected on schedule",             null),
            ("arrived_hub","Chicago, IL",           "Chicago hub — on time",                   """{"hub_code":"ORD03"}"""));

        AddEvents(shipments[8].Id, now.AddHours(-18),
            ("departed",   "Detroit, MI",           "UPS collected",                           null),
            ("in_transit", "Columbus, OH",          "In transit to Charlotte",                 null));

        AddEvents(shipments[9].Id, now.AddDays(-1),
            ("departed",   "Kansas City, MO",       "DHL collected",                           null),
            ("arrived_hub","St. Louis, MO",         "Hub processed",                           null),
            ("in_transit", "Indianapolis, IN",      "En route east",                           null));

        AddEvents(shipments[10].Id, now.AddHours(-12),
            ("departed",   "Salt Lake City, UT",    "USPS origin scan",                        null),
            ("in_transit", "Denver, CO",            "In transit eastbound",                    null));

        AddEvents(shipments[11].Id, now.AddDays(-1),
            ("departed",   "San Diego, CA",         "FedEx collected — heavy freight",         null),
            ("arrived_hub","Los Angeles, CA",       "LAX freight hub",                         """{"hub_code":"LAX02"}"""));

        // Delivered shipments
        AddEvents(shipments[12].Id, now.AddDays(-5),
            ("departed",   "New York, NY",          "Departed JFK air hub",                    null),
            ("arrived_hub","Chicago, IL",           "ORD hub processed",                       null),
            ("delivered",  "Chicago, IL",          "Delivered to recipient",                   """{"signature":"J.Smith","pod_url":"/pods/UPS-2024-013"}"""));

        AddEvents(shipments[13].Id, now.AddDays(-7),
            ("departed",   "Boston, MA",            "DHL collected",                           null),
            ("arrived_hub","Chicago, IL",           "Midwest hub",                             null),
            ("delivered",  "Seattle, WA",          "Delivered — no signature required",        """{"pod_url":"/pods/DHL-2024-014"}"""));

        AddEvents(shipments[14].Id, now.AddDays(-4),
            ("departed",   "Miami, FL",             "USPS origin scan",                        null),
            ("in_transit", "Atlanta, GA",           "In transit via ground",                   null),
            ("delivered",  "Denver, CO",           "Delivered to PO Box",                      """{"pod_url":"/pods/USPS-2024-015"}"""));

        db.ShipmentEvents.AddRange(events);

        // ── ChatSession ────────────────────────────────────────────────────────
        db.ChatSessions.Add(new ChatSession
        {
            Id = Guid.Parse("b0000000-0000-0000-0000-000000000001"),
            SessionName = "Main",
            CreatedAt = now,
            LastActiveAt = now
        });

        await db.SaveChangesAsync();
    }
}
