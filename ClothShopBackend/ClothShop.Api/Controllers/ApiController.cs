using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothShop.Api.Data;
using ClothShop.Api.Models;
using ClothShop.Api.DTOs;

namespace ClothShop.Api.Controllers;

// -------------------------------------------------------------
// 1. PRODUCTS & CATALOG CONTROLLER (/api/products)
// -------------------------------------------------------------
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ClothShopDbContext _db;
    public ProductsController(ClothShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Products.Where(p => p.IsActive).OrderByDescending(p => p.ProductId).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Products.FindAsync(id);
        return p == null ? NotFound() : Ok(p);
    }

    [HttpGet("barcode/{code}")]
    public async Task<IActionResult> GetByBarcode(string code)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => (x.Barcode == code || x.SKU == code) && x.IsActive);
        return p == null ? NotFound(new { message = "Barcode not found" }) : Ok(p);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product prod)
    {
        if (string.IsNullOrWhiteSpace(prod.Barcode))
            prod.Barcode = "890" + DateTime.UtcNow.Ticks.ToString().Substring(8, 9);
        if (string.IsNullOrWhiteSpace(prod.SKU))
            prod.SKU = "SKU-" + new Random().Next(1000, 9999);

        prod.CreatedAt = DateTime.UtcNow;
        prod.UpdatedAt = DateTime.UtcNow;
        _db.Products.Add(prod);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = prod.ProductId }, prod);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product input)
    {
        var existing = await _db.Products.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Barcode = input.Barcode;
        existing.SKU = input.SKU;
        existing.ProductName = input.ProductName;
        existing.Brand = input.Brand;
        existing.Category = input.Category;
        existing.Fabric = input.Fabric;
        existing.Size = input.Size;
        existing.Color = input.Color;
        existing.HsnCode = input.HsnCode;
        existing.CostPrice = input.CostPrice;
        existing.SellingPrice = input.SellingPrice;
        existing.GstRate = input.GstRate;
        existing.StockQuantity = input.StockQuantity;
        existing.MinStockAlert = input.MinStockAlert;
        existing.RackLocation = input.RackLocation;
        existing.Unit = input.Unit;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Products.FindAsync(id);
        if (existing == null) return NotFound();
        existing.IsActive = false;
        await _db.SaveChangesAsync();
        return Ok(new { success = true });
    }
}

// -------------------------------------------------------------
// 2. POS BILLING & CHECKOUT CONTROLLER (/api/billing) [ADDED & FIXED]
// -------------------------------------------------------------
[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly ClothShopDbContext _db;
    public BillingController(ClothShopDbContext db) => _db = db;

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto dto)
    {
        if (dto == null || dto.Items == null || dto.Items.Count == 0)
        {
            return BadRequest(new { message = "Cart cannot be empty." });
        }

        try
        {
            var countToday = await _db.Invoices.CountAsync() + 1;
            var invoiceNo = $"RST-{DateTime.UtcNow:yyyyMMdd}-{countToday:D4}";

            decimal subTotal = 0;
            decimal totalTax = 0;
            decimal cgstTotal = 0;
            decimal sgstTotal = 0;
            decimal igstTotal = 0;

            var invoiceItems = new List<InvoiceItem>();

            foreach (var itemDto in dto.Items)
            {
                var lineGross = itemDto.Quantity * itemDto.UnitPrice;
                var lineTaxable = lineGross - itemDto.DiscountAmount;
                var gstAmount = lineTaxable * (itemDto.GstRate / 100m);

                subTotal += lineGross;
                totalTax += gstAmount;

                decimal cgst = 0, sgst = 0, igst = 0;
                if (dto.IsInterState)
                {
                    igst = gstAmount;
                    igstTotal += gstAmount;
                }
                else
                {
                    cgst = gstAmount / 2m;
                    sgst = gstAmount / 2m;
                    cgstTotal += cgst;
                    sgstTotal += sgst;
                }

                invoiceItems.Add(new InvoiceItem
                {
                    ProductId = itemDto.ProductId,
                    Barcode = itemDto.Barcode ?? string.Empty,
                    ProductName = itemDto.ProductName ?? "Garment Item",
                    Brand = itemDto.Brand ?? "General",
                    Category = itemDto.Category ?? "Apparel",
                    Size = itemDto.Size ?? "M",
                    Color = itemDto.Color ?? "Standard",
                    HsnCode = itemDto.HsnCode ?? "5208",
                    Unit = itemDto.Unit ?? "Pcs",
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    DiscountAmount = itemDto.DiscountAmount,
                    TaxableAmount = lineTaxable,
                    GstRate = itemDto.GstRate,
                    CgstAmount = cgst,
                    SgstAmount = sgst,
                    IgstAmount = igst,
                    LineTotal = lineTaxable + gstAmount
                });

                // Reduce inventory stock in SQL Server
                var product = await _db.Products.FindAsync(itemDto.ProductId);
                if (product != null)
                {
                    product.StockQuantity = Math.Max(0, product.StockQuantity - itemDto.Quantity);
                    product.UpdatedAt = DateTime.UtcNow;
                }
            }

            var grandTotal = Math.Round(subTotal + totalTax);

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNo,
                InvoiceDate = DateTime.UtcNow,
                CustomerName = string.IsNullOrWhiteSpace(dto.CustomerName) ? "Walk-in Customer" : dto.CustomerName,
                CustomerPhone = string.IsNullOrWhiteSpace(dto.CustomerPhone) ? "9841098410" : dto.CustomerPhone,
                CustomerAddress = dto.CustomerAddress,
                CustomerGstin = dto.CustomerGstin,
                IsInterState = dto.IsInterState,
                SubTotal = subTotal,
                ItemDiscountTotal = 0,
                BillDiscountAmount = dto.BillDiscountAmount,
                BillDiscountCode = dto.BillDiscountCode,
                TaxableAmount = subTotal,
                CgstTotal = cgstTotal,
                SgstTotal = sgstTotal,
                IgstTotal = igstTotal,
                TotalTax = totalTax,
                RoundOff = grandTotal - (subTotal + totalTax),
                GrandTotal = grandTotal,
                AmountPaid = dto.AmountPaid > 0 ? dto.AmountPaid : grandTotal,
                BalanceDue = Math.Max(0, grandTotal - (dto.AmountPaid > 0 ? dto.AmountPaid : grandTotal)),
                PaymentStatus = "PAID",
                CashierId = dto.CashierId > 0 ? dto.CashierId : 1,
                CashierName = dto.CashierName ?? "Admin",
                Notes = dto.Notes,
                Items = invoiceItems
            };

            if (dto.Payments != null && dto.Payments.Count > 0)
            {
                foreach (var pay in dto.Payments)
                {
                    invoice.Payments.Add(new InvoicePayment
                    {
                        PaymentMode = pay.Mode ?? "CASH",
                        Amount = pay.Amount,
                        ReferenceNo = pay.ReferenceNo,
                        PaymentDate = DateTime.UtcNow
                    });
                }
            }
            else
            {
                invoice.Payments.Add(new InvoicePayment
                {
                    PaymentMode = "CASH",
                    Amount = grandTotal,
                    PaymentDate = DateTime.UtcNow
                });
            }

            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Return clean JSON without circular references
            return Ok(new
            {
                success = true,
                invoiceId = invoice.InvoiceId,
                invoiceNumber = invoice.InvoiceNumber,
                grandTotal = invoice.GrandTotal,
                subTotal = invoice.SubTotal,
                totalTax = invoice.TotalTax,
                customerName = invoice.CustomerName,
                customerPhone = invoice.CustomerPhone,
                invoiceDate = invoice.InvoiceDate.ToString("yyyy-MM-ddTHH:mm:ss")
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var list = await _db.Invoices
            .OrderByDescending(i => i.InvoiceId)
            .Take(50)
            .Select(i => new
            {
                invoiceId = i.InvoiceId,
                invoiceNumber = i.InvoiceNumber,
                invoiceDate = i.InvoiceDate,
                customerName = i.CustomerName,
                customerPhone = i.CustomerPhone,
                grandTotal = i.GrandTotal,
                paymentStatus = i.PaymentStatus,
                items = i.Items.Select(it => new
                {
                    it.ProductName,
                    it.Size,
                    it.Color,
                    it.Quantity,
                    it.UnitPrice,
                    it.GstRate,
                    it.LineTotal
                }).ToList()
            })
            .ToListAsync();

        return Ok(list);
    }
}

// -------------------------------------------------------------
// 3. INVENTORY & STOCK AUDIT CONTROLLER (/api/stock)
// -------------------------------------------------------------
[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly ClothShopDbContext _db;
    public StockController(ClothShopDbContext db) => _db = db;

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock() =>
        Ok(await _db.Products.Where(p => p.StockQuantity <= p.MinStockAlert && p.IsActive).ToListAsync());

    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] StockAdjustment adj)
    {
        var prod = await _db.Products.FindAsync(adj.ProductId);
        if (prod == null) return NotFound("Product not found");

        adj.PreviousStock = prod.StockQuantity;
        adj.ChangeQuantity = adj.NewStock - prod.StockQuantity;
        adj.Barcode = prod.Barcode;
        adj.AdjustmentDate = DateTime.UtcNow;

        prod.StockQuantity = adj.NewStock;
        prod.UpdatedAt = DateTime.UtcNow;

        _db.StockAdjustments.Add(adj);
        await _db.SaveChangesAsync();
        return Ok(adj);
    }

    [HttpGet("adjustments")]
    public async Task<IActionResult> GetAdjustments() =>
        Ok(await _db.StockAdjustments.OrderByDescending(a => a.AdjustmentDate).Take(50).ToListAsync());
}

// -------------------------------------------------------------
// 4. PURCHASES & INWARD CONTROLLER (/api/purchases)
// -------------------------------------------------------------
[ApiController]
[Route("api/[controller]")]
public class PurchasesController : ControllerBase
{
    private readonly ClothShopDbContext _db;
    public PurchasesController(ClothShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Purchases.Include(p => p.Items).OrderByDescending(p => p.PurchaseDate).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> CreatePurchase([FromBody] Purchase purchase)
    {
        purchase.PurchaseNumber = "PUR-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        purchase.PurchaseDate = DateTime.UtcNow;

        foreach (var item in purchase.Items)
        {
            var p = await _db.Products.FindAsync(item.ProductId);
            if (p != null)
            {
                p.StockQuantity += item.Quantity;
                p.CostPrice = item.PurchaseRate;
            }
        }

        var sup = await _db.Suppliers.FindAsync(purchase.SupplierId);
        if (sup != null)
        {
            sup.CreditBalance += (purchase.GrandTotal - purchase.AmountPaid);
        }

        _db.Purchases.Add(purchase);
        await _db.SaveChangesAsync();
        return Ok(purchase);
    }
}

// -------------------------------------------------------------
// 5. SALES RETURNS & EXCHANGES CONTROLLER (/api/returns)
// -------------------------------------------------------------
[ApiController]
[Route("api/[controller]")]
public class ReturnsController : ControllerBase
{
    private readonly ClothShopDbContext _db;
    public ReturnsController(ClothShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.SalesReturns.OrderByDescending(r => r.ReturnDate).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> ProcessReturn([FromBody] SalesReturn ret)
    {
        ret.ReturnNumber = "RET-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        ret.ReturnDate = DateTime.UtcNow;
        ret.NetRefundAmount = ret.ReturnedValue - ret.ExchangeValue;

        _db.SalesReturns.Add(ret);
        await _db.SaveChangesAsync();
        return Ok(ret);
    }
}

// -------------------------------------------------------------
// 6. CUSTOMER KHATA & LOYALTY CONTROLLER (/api/customers)
// -------------------------------------------------------------
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ClothShopDbContext _db;
    public CustomersController(ClothShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Customers.OrderBy(c => c.FullName).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Customer cust)
    {
        _db.Customers.Add(cust);
        await _db.SaveChangesAsync();
        return Ok(cust);
    }

    [HttpPost("{id}/payment")]
    public async Task<IActionResult> CollectCredit(int id, [FromBody] decimal amount)
    {
        var cust = await _db.Customers.FindAsync(id);
        if (cust == null) return NotFound();

        cust.CreditBalance = Math.Max(0, cust.CreditBalance - amount);
        await _db.SaveChangesAsync();
        return Ok(cust);
    }
}

// -------------------------------------------------------------
// 7. SUPPLIERS & MILLS CONTROLLER (/api/suppliers)
// -------------------------------------------------------------
[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ClothShopDbContext _db;
    public SuppliersController(ClothShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Suppliers.OrderBy(s => s.SupplierName).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Supplier sup)
    {
        _db.Suppliers.Add(sup);
        await _db.SaveChangesAsync();
        return Ok(sup);
    }

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> SettlePayment(int id, [FromBody] decimal amount)
    {
        var sup = await _db.Suppliers.FindAsync(id);
        if (sup == null) return NotFound();

        sup.CreditBalance = Math.Max(0, sup.CreditBalance - amount);
        await _db.SaveChangesAsync();
        return Ok(sup);
    }
}

// -------------------------------------------------------------
// 8. EXPENSES CONTROLLER (/api/expenses)
// -------------------------------------------------------------
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly ClothShopDbContext _db;
    public ExpensesController(ClothShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Expenses.OrderByDescending(e => e.ExpenseDate).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Expense exp)
    {
        exp.ExpenseNumber = "EXP-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        exp.ExpenseDate = DateTime.UtcNow;
        _db.Expenses.Add(exp);
        await _db.SaveChangesAsync();
        return Ok(exp);
    }
}

// -------------------------------------------------------------
// 9. DISCOUNTS & OFFERS CONTROLLER (/api/discounts)
// -------------------------------------------------------------
[ApiController]
[Route("api/[controller]")]
public class DiscountsController : ControllerBase
{
    private readonly ClothShopDbContext _db;
    public DiscountsController(ClothShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.DiscountOffers.Where(d => d.IsActive).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DiscountOffer offer)
    {
        _db.DiscountOffers.Add(offer);
        await _db.SaveChangesAsync();
        return Ok(offer);
    }

    [HttpGet("validate/{code}")]
    public async Task<IActionResult> Validate(string code)
    {
        var offer = await _db.DiscountOffers.FirstOrDefaultAsync(o => o.OfferCode == code && o.IsActive);
        return offer == null ? NotFound(new { message = "Invalid coupon code" }) : Ok(offer);
    }
}

// -------------------------------------------------------------
// 10. REPORTS, METRICS & P&L CONTROLLER (/api/reports)
// -------------------------------------------------------------
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ClothShopDbContext _db;
    public ReportsController(ClothShopDbContext db) => _db = db;

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var today = DateTime.UtcNow.Date;
        var todayInvoices = await _db.Invoices.Where(i => i.InvoiceDate >= today).ToListAsync();
        var todayExpenses = await _db.Expenses.Where(e => e.ExpenseDate >= today).SumAsync(e => e.Amount);

        decimal totalSales = todayInvoices.Sum(i => i.GrandTotal);
        decimal totalTax = todayInvoices.Sum(i => i.TotalTax);
        int billsCount = todayInvoices.Count;

        int lowStockCount = await _db.Products.CountAsync(p => p.StockQuantity <= p.MinStockAlert && p.IsActive);
        decimal customerOutstanding = await _db.Customers.SumAsync(c => c.CreditBalance);
        decimal supplierPayable = await _db.Suppliers.SumAsync(s => s.CreditBalance);

        return Ok(new
        {
            todaySales = totalSales,
            todayBills = billsCount,
            todayGst = totalTax,
            todayExpenses = todayExpenses,
            netProfitEstimate = (totalSales * 0.28m) - todayExpenses,
            lowStockItems = lowStockCount,
            totalCustomerDue = customerOutstanding,
            totalSupplierDue = supplierPayable
        });
    }
}

// -------------------------------------------------------------
// 11. USERS / STAFF CONTROLLER (/api/users)
// -------------------------------------------------------------
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ClothShopDbContext _db;
    public UsersController(ClothShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Users.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(user);
    }
}

// -------------------------------------------------------------
// 12. SHOP SETTINGS CONTROLLER (/api/settings)
// -------------------------------------------------------------
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ClothShopDbContext _db;
    public SettingsController(ClothShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await _db.ShopSettings.FirstOrDefaultAsync() ?? new ShopSetting());

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] ShopSetting setting)
    {
        var existing = await _db.ShopSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            _db.ShopSettings.Add(setting);
        }
        else
        {
            existing.ShopName = setting.ShopName;
            existing.Tagline = setting.Tagline;
            existing.AddressLine = setting.AddressLine;
            existing.City = setting.City;
            existing.Phone = setting.Phone;
            existing.GSTIN = setting.GSTIN;
            existing.UpiId = setting.UpiId;
            existing.InvoicePrefix = setting.InvoicePrefix;
            existing.DefaultGstRate = setting.DefaultGstRate;
            existing.TermsAndConditions = setting.TermsAndConditions;
        }
        await _db.SaveChangesAsync();
        return Ok(setting);
    }
}