using LogisticsAI.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAI.Tests;

// This static helper creates a fresh in-memory database for each test.
// "static" means you never create an instance of this class — you just call its methods directly.
public static class TestDbHelper
{
    // DbContextOptions tells EF Core how to connect to a database.
    // Instead of real Postgres, we use UseInMemoryDatabase — a fake DB that lives in memory.
    // Each test passes a unique name so tests never accidentally share data.
    public static AppDbContext CreateDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        // AppDbContext is the EF Core class that represents our database connection.
        // We pass in the options we just built.
        return new AppDbContext(options);
    }
}
