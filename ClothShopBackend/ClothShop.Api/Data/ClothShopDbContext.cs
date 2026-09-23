using Microsoft.EntityFrameworkCore;
using ClothShop.Api.Models;

namespace ClothShop.Api.Data;

public class ClothShopDbContext : DbContext
{
    public ClothShopDbContext(DbContextOptions<ClothShopDbContext> options) : base(options) { }

    public DbSet<ShopSetting> ShopSettings => Set<ShopSetting>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<DiscountOffer> DiscountOffers => Set<DiscountOffer>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Items)
            .WithOne()
            .HasForeignKey(item => item.InvoiceId);

        modelBuilder.Entity<Purchase>()
            .HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(item => item.PurchaseId);
    }
}