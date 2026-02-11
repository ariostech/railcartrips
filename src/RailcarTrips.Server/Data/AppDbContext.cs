using Microsoft.EntityFrameworkCore;
using RailcarTrips.Server.Data.Entities;

namespace RailcarTrips.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<City> Cities => Set<City>();
    public DbSet<EventCodeDefinition> EventCodeDefinitions => Set<EventCodeDefinition>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<EquipmentEvent> EquipmentEvents => Set<EquipmentEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.TimeZone).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<EventCodeDefinition>(entity =>
        {
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasMaxLength(1);
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.EquipmentId);

            entity.HasOne(t => t.OriginCity)
                  .WithMany()
                  .HasForeignKey(t => t.OriginCityId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.DestinationCity)
                  .WithMany()
                  .HasForeignKey(t => t.DestinationCityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EquipmentEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EquipmentId);

            entity.HasOne(e => e.City)
                  .WithMany()
                  .HasForeignKey(e => e.CityId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Trip)
                  .WithMany(t => t.Events)
                  .HasForeignKey(e => e.TripId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        SeedCities(modelBuilder);
        SeedEventCodeDefinitions(modelBuilder);
    }

    private static void SeedCities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>().HasData(
            new City { Id = 1, Name = "Vancouver", TimeZone = "Pacific Standard Time" },
            new City { Id = 2, Name = "Victoria", TimeZone = "Pacific Standard Time" },
            new City { Id = 3, Name = "Kelowna", TimeZone = "Pacific Standard Time" },
            new City { Id = 4, Name = "Kamloops", TimeZone = "Pacific Standard Time" },
            new City { Id = 5, Name = "Prince George", TimeZone = "Pacific Standard Time" },
            new City { Id = 6, Name = "Calgary", TimeZone = "Mountain Standard Time" },
            new City { Id = 7, Name = "Edmonton", TimeZone = "Mountain Standard Time" },
            new City { Id = 8, Name = "Lethbridge", TimeZone = "Mountain Standard Time" },
            new City { Id = 9, Name = "Red Deer", TimeZone = "Mountain Standard Time" },
            new City { Id = 10, Name = "Fort McMurray", TimeZone = "Mountain Standard Time" },
            new City { Id = 11, Name = "Regina", TimeZone = "Canada Central Standard Time" },
            new City { Id = 12, Name = "Saskatoon", TimeZone = "Canada Central Standard Time" },
            new City { Id = 13, Name = "Moose Jaw", TimeZone = "Canada Central Standard Time" },
            new City { Id = 14, Name = "Brandon", TimeZone = "Central Standard Time" },
            new City { Id = 15, Name = "Winnipeg", TimeZone = "Central Standard Time" },
            new City { Id = 16, Name = "Thunder Bay", TimeZone = "Eastern Standard Time" },
            new City { Id = 17, Name = "Sault Ste. Marie", TimeZone = "Eastern Standard Time" },
            new City { Id = 18, Name = "Sudbury", TimeZone = "Eastern Standard Time" },
            new City { Id = 19, Name = "North Bay", TimeZone = "Eastern Standard Time" },
            new City { Id = 20, Name = "Barrie", TimeZone = "Eastern Standard Time" },
            new City { Id = 21, Name = "Toronto", TimeZone = "Eastern Standard Time" },
            new City { Id = 22, Name = "Mississauga", TimeZone = "Eastern Standard Time" },
            new City { Id = 23, Name = "Hamilton", TimeZone = "Eastern Standard Time" },
            new City { Id = 24, Name = "London", TimeZone = "Eastern Standard Time" },
            new City { Id = 25, Name = "Kitchener", TimeZone = "Eastern Standard Time" },
            new City { Id = 26, Name = "Windsor", TimeZone = "Eastern Standard Time" },
            new City { Id = 27, Name = "St. Catharines", TimeZone = "Eastern Standard Time" },
            new City { Id = 28, Name = "Oshawa", TimeZone = "Eastern Standard Time" },
            new City { Id = 29, Name = "Kingston", TimeZone = "Eastern Standard Time" },
            new City { Id = 30, Name = "Ottawa", TimeZone = "Eastern Standard Time" },
            new City { Id = 31, Name = "Gatineau", TimeZone = "Eastern Standard Time" },
            new City { Id = 32, Name = "Montreal", TimeZone = "Eastern Standard Time" },
            new City { Id = 33, Name = "Quebec City", TimeZone = "Eastern Standard Time" },
            new City { Id = 34, Name = "Sherbrooke", TimeZone = "Eastern Standard Time" },
            new City { Id = 35, Name = "Trois-Rivières", TimeZone = "Eastern Standard Time" },
            new City { Id = 36, Name = "Saguenay", TimeZone = "Eastern Standard Time" },
            new City { Id = 37, Name = "Rimouski", TimeZone = "Eastern Standard Time" },
            new City { Id = 38, Name = "Edmundston", TimeZone = "Atlantic Standard Time" },
            new City { Id = 39, Name = "Fredericton", TimeZone = "Atlantic Standard Time" },
            new City { Id = 40, Name = "Moncton", TimeZone = "Atlantic Standard Time" },
            new City { Id = 41, Name = "Saint John", TimeZone = "Atlantic Standard Time" },
            new City { Id = 42, Name = "Bathurst", TimeZone = "Atlantic Standard Time" },
            new City { Id = 43, Name = "Charlottetown", TimeZone = "Atlantic Standard Time" },
            new City { Id = 44, Name = "Summerside", TimeZone = "Atlantic Standard Time" },
            new City { Id = 45, Name = "Sydney", TimeZone = "Atlantic Standard Time" },
            new City { Id = 46, Name = "Truro", TimeZone = "Atlantic Standard Time" },
            new City { Id = 47, Name = "New Glasgow", TimeZone = "Atlantic Standard Time" },
            new City { Id = 48, Name = "Dartmouth", TimeZone = "Atlantic Standard Time" },
            new City { Id = 49, Name = "Halifax", TimeZone = "Atlantic Standard Time" }
        );
    }

    private static void SeedEventCodeDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventCodeDefinition>().HasData(
            new EventCodeDefinition { Code = "W", Description = "Released", LongDescription = "Railcar equipment is released from origin" },
            new EventCodeDefinition { Code = "A", Description = "Arrived", LongDescription = "Railcar equipment arrives at a city on route" },
            new EventCodeDefinition { Code = "D", Description = "Departed", LongDescription = "Railcar equipment departs from a city on route" },
            new EventCodeDefinition { Code = "Z", Description = "Placed", LongDescription = "Railcar equipment is placed at destination" }
        );
    }
}
