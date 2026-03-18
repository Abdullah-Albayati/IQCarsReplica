using HotelSystemBackend.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelSystemBackend.Data;

public class  ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // Add your DbSets here
    public DbSet<Hotel> Hotels { get; set; }

    public DbSet<User> Users { get; set; }
    // Example: public DbSet<Product> Products { get; set; }
}
