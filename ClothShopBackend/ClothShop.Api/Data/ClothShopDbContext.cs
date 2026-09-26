using Microsoft.EntityFrameworkCore;
using ClothShop.Api.Models;

namespace ClothShop.Api.Data;

public class ClothShopDbContext : DbContext
{
    public ClothShopDbContext(DbContextOptions<ClothShopDbContext> options) : base(options)
    {
    }

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

    // 1. FIXES ALL 30+ DECIMAL STORE TYPE WARNINGS AUTOMATICALLY
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    // 2. FIXES THE SQL SERVER TRIGGER / OUTPUT CLAUSE ISSUE
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tell SQL Server not to use the OUTPUT clause which triggers block
        modelBuilder.Entity<Invoice>(b =>
        {
            b.ToTable("Invoices", tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<InvoiceItem>(b =>
        {
            b.ToTable("InvoiceItems", tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<InvoicePayment>(b =>
        {
            b.ToTable("InvoicePayments", tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("Products", tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<Customer>(b =>
        {
            b.ToTable("Customers", tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<Supplier>(b =>
        {
            b.ToTable("Suppliers", tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<Purchase>(b =>
        {
            b.ToTable("Purchases", tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<PurchaseItem>(b =>
        {
            b.ToTable("PurchaseItems", tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<Expense>(b =>
        {
            b.ToTable("Expenses", tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<SalesReturn>(b =>
        {
            b.ToTable("SalesReturns", tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<StockAdjustment>(b =>
        {
            b.ToTable("StockAdjustments", tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<DiscountOffer>(b =>
        {
            b.ToTable("DiscountOffers", tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<ShopSetting>(b =>
        {
            b.ToTable("ShopSettings", tb => tb.UseSqlOutputClause(false));
        });

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("Users", tb => tb.UseSqlOutputClause(false));
        });
    }
}