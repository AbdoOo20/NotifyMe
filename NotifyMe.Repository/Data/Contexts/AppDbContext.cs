using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotifyMe.Core.Entities;

namespace NotifyMe.Repository.Data.Contexts
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ***** Customer ***** //
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");

                entity.HasKey(c => c.Id);

                entity.HasMany(c => c.Monitors)
                      .WithOne(m => m.Customer)
                      .HasForeignKey(m => m.CustomerID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ***** Monitor ***** //
            modelBuilder.Entity<Core.Entities.Monitor>(entity =>
            {
                entity.ToTable("Monitors");

                entity.HasKey(m => m.Id);

                entity.Property(m => m.URL)
                      .IsRequired()
                      .HasMaxLength(2048);

                entity.Property(m => m.CssSelector)
                      .HasMaxLength(255);

                entity.Property(m => m.LastContentHash)
                      .HasMaxLength(256);

                entity.Property(m => m.CheckIntervalMinutes)
                      .IsRequired();

                entity.HasIndex(m => new { m.CustomerID, m.IsActive });

                entity.HasMany(m => m.Alerts)
                      .WithOne(a => a.Monitor)
                      .HasForeignKey(a => a.MonitorID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ***** Alert ***** //
            modelBuilder.Entity<Alert>(entity =>
            {
                entity.ToTable("Alerts");

                entity.HasKey(a => a.Id);

                entity.Property(a => a.OldHash)
                      .HasMaxLength(256);

                entity.Property(a => a.NewHash)
                      .HasMaxLength(256);

                entity.Property(a => a.ChangeSummary)
                      .HasMaxLength(1000);

                entity.Property(a => a.SentVia)
                      .HasConversion<int>();

                entity.HasIndex(a => a.MonitorID);
            });

            // ***** Subscriptions ***** //
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("Subscriptions");

                entity.HasKey(a => a.Id);

                entity.HasIndex(m => new { m.CustomerID, m.IsActive, m.SubscriptionStatus });

                entity.Ignore(s => s.IsActive);
            });
        }

        public virtual DbSet<Customer> Customers { get; set; }

        public virtual DbSet<Core.Entities.Monitor> Monitors { get; set; }

        public virtual DbSet<Alert> Alerts { get; set; }

        public virtual DbSet<Subscription> Subscriptions { get; set; }

        public virtual DbSet<Notification> Notifications { get; set; }

        public virtual DbSet<MonitorErrors> MonitorErrors { get; set; }
    }
}