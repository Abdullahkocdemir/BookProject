using EntityLayer.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Context
{
    public class ETicaretDb : IdentityDbContext<AppUser, AppRole, string>
    {
        public ETicaretDb(DbContextOptions<ETicaretDb> options) : base(options)
        {
        }

        // DbSet'ler (tablolar)
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Testimonial> Testimonials { get; set; }

        // Identity için tablolar
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppRole> AppRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Identity konfigürasyonları (varsa) burada yapılır
        }
    }
}
