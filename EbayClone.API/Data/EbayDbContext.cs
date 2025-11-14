using Microsoft.EntityFrameworkCore;
using EbayClone.API.Models;

namespace EbayClone.API.Models
{
    public class EbayDbContext : DbContext
    {
        public EbayDbContext(DbContextOptions<EbayDbContext> options) : base(options)
        {
        }

        // === DbSet Definitions ===
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PaymentInfo> Payments { get; set; }
        public DbSet<ShippingInfo> Shippings { get; set; }
        public DbSet<ShippingEvent> ShippingEvents { get; set; }
        public DbSet<EscrowInfo> Escrows { get; set; }
        public DbSet<ReturnRequest> ReturnRequests { get; set; }
        public DbSet<ShippingRegion> ShippingRegions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== USER =====
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.Products)
                .WithOne(p => p.Seller)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== PRODUCT =====
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Category>()
            .HasMany(c => c.Products)
            .WithOne()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

            // ===== ORDER =====
            // Buyer
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<PaymentInfo>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Shipping)
                .WithOne(s => s.Order)
                .HasForeignKey<ShippingInfo>(s => s.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Escrow)
                .WithOne(e => e.Order)
                .HasForeignKey<EscrowInfo>(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            // ===== ORDER ITEM =====
            modelBuilder.Entity<OrderItem>()
                .Property(i => i.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== PAYMENT INFO =====
            modelBuilder.Entity<PaymentInfo>()
                .Property(p => p.Status)
                .HasConversion<string>();

            // ===== SHIPPING INFO =====
            modelBuilder.Entity<ShippingInfo>()
                .HasMany(s => s.Events)
                .WithOne(e => e.ShippingInfo)
                .HasForeignKey(e => e.ShippingInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== ESCROW INFO =====
            modelBuilder.Entity<EscrowInfo>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);

            // ===== RETURN REQUEST =====
            modelBuilder.Entity<ReturnRequest>()
                .HasOne(r => r.Order)
                .WithMany(o => o.ReturnRequests)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== SEED ADMIN ACCOUNT =====
            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = Guid.NewGuid().ToString(),
                UserName = "Admin",
                Email = "admin@ebayclone.com",
                PasswordHash = "admin123", // ⚠️ Hash khi chạy thật
                Role = "Admin",
                TrustLevel = SellerTrustLevel.Diamond,
                Rating = 5.0m,
                TotalSales = 0
            });
        }
    }
}
