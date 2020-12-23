using ApiService.Models;
using Microsoft.EntityFrameworkCore;
using Models;

namespace ApiService.Contexts
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        { }

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
