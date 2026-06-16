using LogisticsAI.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAI.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<Models.Route> Routes => Set<Models.Route>();
    public DbSet<ShipmentEvent> ShipmentEvents => Set<ShipmentEvent>();
    public DbSet<RcaResult> RcaResults => Set<RcaResult>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shipment>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => s.TrackingNumber).IsUnique();
            e.Property(s => s.Status).HasConversion<string>();
            e.Property(s => s.WeightKg).HasPrecision(10, 2);
            e.Property(s => s.DelayHours).HasPrecision(8, 2);
        });

        modelBuilder.Entity<Models.Route>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.Shipment)
             .WithMany(s => s.Routes)
             .HasForeignKey(r => r.ShipmentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShipmentEvent>(e =>
        {
            e.HasKey(ev => ev.Id);
            e.HasOne(ev => ev.Shipment)
             .WithMany(s => s.Events)
             .HasForeignKey(ev => ev.ShipmentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RcaResult>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.ConfidenceScore).HasPrecision(5, 4);
            e.HasOne(r => r.Shipment)
             .WithMany(s => s.RcaResults)
             .HasForeignKey(r => r.ShipmentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatSession>(e =>
        {
            e.HasKey(cs => cs.Id);
        });

        modelBuilder.Entity<ChatMessage>(e =>
        {
            e.HasKey(cm => cm.Id);
            e.HasOne(cm => cm.Session)
             .WithMany(cs => cs.Messages)
             .HasForeignKey(cm => cm.SessionId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
