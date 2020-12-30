using ApiInABox.Models;
using ApiInABox.Models.Auth;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiInABox.Contexts
{
    public class DatabaseContext : DbContext
    {
        public string TokenNameId { get; set; } = "sys";

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        { }

        public override int SaveChanges()
        {
            SetTrackingData();

            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            SetTrackingData();

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, 
            CancellationToken cancellationToken = default)
        {
            SetTrackingData();

            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTrackingData();

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetTrackingData()
        {
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Modified &&
                     e.Entity is AbstractDbBase))
            {
                if (entry.Entity is not AbstractDbBase ae)
                    continue;

                ae.UpdatedDate = Instant.FromDateTimeUtc(DateTime.UtcNow);
                ae.UpdatedBy = TokenNameId;
            }

            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added &&
                     e.Entity is AbstractDbBase))
            {
                if (entry.Entity is not AbstractDbBase ae)
                    continue;

                ae.CreatedBy = TokenNameId;
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Ignore<AbstractDbBase>();

            builder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            builder.Entity<User>()
                .HasIndex(u => u.ActivationEmail)
                .IsUnique();

            builder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithOne(r => r.User)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<User>()
                .UseXminAsConcurrencyToken();

            builder.Entity<Role>()
                .HasIndex(r => r.Name);

            builder.Entity<Role>()
                .UseXminAsConcurrencyToken();

            builder.Entity<ApiKey>()
                .HasIndex(r => r.Name)
                .IsUnique();

            builder.Entity<ApiKey>()
                .HasIndex(r => r.Key)
                .IsUnique();

            builder.Entity<ApiKey>()
                .UseXminAsConcurrencyToken();
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<ApiKey> ApiKeys { get; set; }
    }
}
