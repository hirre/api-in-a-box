using ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Contexts
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.Ignore<AbstractDbBase>();
            //builder.Entity<DatabaseContext>()
            //       .UseXminAsConcurrencyToken();

            builder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();
        }

        public virtual DbSet<User> Users { get; set; }
    }
}
