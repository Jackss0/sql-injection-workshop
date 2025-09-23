using Microsoft.EntityFrameworkCore;
using SqlInjectionWorkshop.Models;

namespace SqlInjectionWorkshop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data para demostración
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "admin123", Email = "admin@demo.com", IsAdmin = true },
                new User { Id = 2, Username = "user1", Password = "password123", Email = "user1@demo.com", IsAdmin = false },
                new User { Id = 3, Username = "test", Password = "test123", Email = "test@demo.com", IsAdmin = false }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", Description = "Laptop gaming", Price = 999.99m, Category = "Electronics" },
                new Product { Id = 2, Name = "Mouse", Description = "Mouse inalámbrico", Price = 29.99m, Category = "Electronics" },
                new Product { Id = 3, Name = "Teclado", Description = "Teclado mecánico", Price = 89.99m, Category = "Electronics" }
            );

            modelBuilder.Entity<Comment>().HasData(
                new Comment { Id = 1, Content = "Excelente producto", Author = "user1", IsApproved = true },
                new Comment { Id = 2, Content = "Muy buena calidad", Author = "test", IsApproved = true },
                new Comment { Id = 3, Content = "Pendiente de aprobación", Author = "user1", IsApproved = false }
            );
        }
    }
}
