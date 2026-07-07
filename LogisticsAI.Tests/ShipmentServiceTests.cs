using LogisticsAI.Api.Models;
using LogisticsAI.Api.Services;

namespace LogisticsAI.Tests;

// A test class is just a plain C# class — no special base class needed in xUnit.
// Each public method marked with [Fact] is one test.
public class ShipmentServiceTests
{
    // ── Helper ────────────────────────────────────────────────────────────────

    // This private method creates a minimal valid Shipment object.
    // We reuse it in every test so we don't repeat the same setup code.
    // "private" means only this class can call it.
    private static Shipment MakeShipment(string trackingNumber) => new()
    {
        // Guid.NewGuid() generates a random unique ID each time it's called.
        Id              = Guid.NewGuid(),
        TrackingNumber  = trackingNumber,
        Carrier         = "FedEx",
        OriginCity      = "Los Angeles, CA",
        DestinationCity = "New York, NY",
        Status          = ShipmentStatus.InTransit,
        WeightKg        = 10m,           // "m" suffix means decimal (not double)
        ScheduledEta    = DateTime.UtcNow.AddDays(3),
        DelayHours      = 0m,
        CreatedAt       = DateTime.UtcNow
    };

    // ── Test 1 ────────────────────────────────────────────────────────────────

    // [Fact] is the xUnit attribute that marks a method as a test.
    // Without it, xUnit won't discover or run this method.
    [Fact]
    // Test method names follow the pattern: MethodName_ExpectedResult_WhenCondition
    // This makes it clear what is being tested just by reading the name.
    public async Task GetAllAsync_ReturnsAllShipments()
    {
        // ── Arrange ───────────────────────────────────────────────────────────
        // "Arrange" = set up everything the test needs before running.

        // Each test gets its own in-memory database by passing a unique name.
        // nameof(GetAllAsync_ReturnsAllShipments) just returns the string
        // "GetAllAsync_ReturnsAllShipments" — a convenient way to get a unique name.
        var db = TestDbHelper.CreateDb(nameof(GetAllAsync_ReturnsAllShipments));

        // Add two shipments directly to the database.
        db.Shipments.Add(MakeShipment("TRK-001"));
        db.Shipments.Add(MakeShipment("TRK-002"));

        // SaveChangesAsync actually writes to the in-memory database.
        // Without this, the data is staged but not persisted yet.
        // "await" means: wait for this async operation to complete before continuing.
        await db.SaveChangesAsync();

        // Create the service we want to test, passing in our test database.
        var service = new ShipmentService(db);

        // ── Act ───────────────────────────────────────────────────────────────
        // "Act" = call the method we are testing.

        var result = await service.GetAllAsync();

        // ── Assert ────────────────────────────────────────────────────────────
        // "Assert" = check that the result is what we expected.

        // Assert.Equal(expected, actual) — fails the test if they don't match.
        // We seeded 2 shipments, so we expect 2 back.
        // .Count() counts the items in the returned list.
        Assert.Equal(2, result.Count());
    }

    // ── Test 2 ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsShipment_WhenExists()
    {
        // ── Arrange ───────────────────────────────────────────────────────────

        var db = TestDbHelper.CreateDb(nameof(GetByIdAsync_ReturnsShipment_WhenExists));

        // Create one shipment and remember its Id so we can fetch it later.
        var shipment = MakeShipment("TRK-100");
        db.Shipments.Add(shipment);
        await db.SaveChangesAsync();

        var service = new ShipmentService(db);

        // ── Act ───────────────────────────────────────────────────────────────

        // Fetch the shipment by its Id.
        // The return type is Shipment? — the "?" means it could be null if not found.
        var result = await service.GetByIdAsync(shipment.Id);

        // ── Assert ────────────────────────────────────────────────────────────

        // Assert.NotNull checks that result is not null.
        // If it IS null, the test fails here with a clear message.
        Assert.NotNull(result);

        // Assert.Equal checks the tracking number matches what we put in.
        // This confirms we got back the right shipment, not just any shipment.
        Assert.Equal("TRK-100", result.TrackingNumber);
    }

    // ── Test 3 ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // ── Arrange ───────────────────────────────────────────────────────────

        // Empty database — no shipments seeded at all.
        var db = TestDbHelper.CreateDb(nameof(GetByIdAsync_ReturnsNull_WhenNotFound));
        var service = new ShipmentService(db);

        // ── Act ───────────────────────────────────────────────────────────────

        // Try to fetch a shipment with a random ID that doesn't exist.
        var result = await service.GetByIdAsync(Guid.NewGuid());

        // ── Assert ────────────────────────────────────────────────────────────

        // Assert.Null checks that result IS null.
        // ShipmentService.GetByIdAsync should return null when nothing is found.
        Assert.Null(result);
    }
}
