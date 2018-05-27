using Microsoft.EntityFrameworkCore;
using MyRestful.Core.DomainModels;
using MyRestful.Infrastructure.EntityConfigurations;

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
            base.OnModelCreating(modelBuilder); // 在父类方法里，它什么也没做

            modelBuilder.ApplyConfiguration(new CountryConfiguration());
            modelBuilder.ApplyConfiguration(new CityConfiguration());
        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }

    }
}
