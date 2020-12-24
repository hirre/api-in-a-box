using ApiService.Models;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiService.Contexts
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        { }

        public override int SaveChanges()
        {
            SetUpdatedDateTime();

            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            SetUpdatedDateTime();

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, 
            CancellationToken cancellationToken = default)
        {
            SetUpdatedDateTime();

            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetUpdatedDateTime();

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetUpdatedDateTime()
        {
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Modified &&
                     e.Entity is AbstractDbBase))
            {
                if (entry.Entity is not AbstractDbBase ae)
                    continue;

                ae.UpdatedDate = DateTimeOffset.UtcNow;
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Ignore<AbstractDbBase>();

            builder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            builder.Entity<User>()
                .UseXminAsConcurrencyToken();

            builder.Entity<Role>()
                .HasIndex(r => r.Name);

            builder.Entity<Role>()
                .UseXminAsConcurrencyToken();
        }

        public virtual DbSet<User> Users { get; set; }
    }
}
