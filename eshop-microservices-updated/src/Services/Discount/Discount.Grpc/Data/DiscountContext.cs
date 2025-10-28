using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Data;

public class DiscountContext(DbContextOptions<DiscountContext> options) : DbContext(options)
{
    public DbSet<Coupon> Coupons { get; set; } = default!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coupon>().HasData(
            new Coupon { Id = 1, ProductName = "Iphone X", Description = "Iphone X Discount", Amount = 150 },
            new Coupon { Id = 2, ProductName = "Iphone 12", Description = "Iphone 12 Discount", Amount = 450 }
        );
    }
}
