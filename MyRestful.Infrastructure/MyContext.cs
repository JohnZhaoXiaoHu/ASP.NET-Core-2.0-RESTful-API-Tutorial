using Microsoft.EntityFrameworkCore;
using MyRestful.Core.DomainModels;

namespace MyRestful.Infrastructure
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions<MyContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }

    }
}
