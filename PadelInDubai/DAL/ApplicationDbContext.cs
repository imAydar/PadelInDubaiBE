using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PadelInDubai.DAL.Entities;

namespace PadelInDubai.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientTag> ClientTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Converter for non-nullable DateTime
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );

            // Converter for nullable DateTime
            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
            );

            // Loop through all entities and all properties
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }

            modelBuilder.Entity<Event>()
                .HasIndex(e => new { e.ServiceId, e.StaffId, e.Date })
                .IsUnique();

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Service)
                .WithMany(s => s.Events)
                .HasForeignKey(e => e.ServiceId);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Staff)
                .WithMany(s => s.Events)
                .HasForeignKey(e => e.StaffId);

            modelBuilder.Entity<Record>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Records)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade); // or .Restrict based on your rules
            //modelBuilder.Entity<Record>()
            //    .HasOne<Event>()
            //    .WithMany()
            //    .HasForeignKey(r => r.EventId);
            //modelBuilder.Entity<Record>()
            //    .HasOne<Event>()
            //    .WithMany(e => e.Records)
            //    .HasForeignKey(r => r.ActivityId)
            //    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
