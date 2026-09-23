using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothShop.Api.Models;

[Table("ShopSettings")]
public class ShopSetting
{
    [Key]
    public int SettingId { get; set; }
    public string ShopName { get; set; } = "Royal Silk & Textiles";
    public string? Tagline { get; set; } = "Traditional Silks, Ready-mades & Suiting";
    public string AddressLine { get; set; } = "42, Gandhi Road";
    public string City { get; set; } = "Chennai";
    public string StateName { get; set; } = "Tamil Nadu";
    public string StateCode { get; set; } = "33";
    public string Pincode { get; set; } = "600001";
    public string Phone { get; set; } = "+91 98401 23456";
    public string? Email { get; set; } = "contact@royalsilk.com";
    public string GSTIN { get; set; } = "33AAAAA0000A1Z5";
    public string? UpiId { get; set; } = "royalsilk@icici";
    public string? BankName { get; set; } = "SBI";
    public string? BankAccountNumber { get; set; } = "389201992019";
    public string? BankIfsc { get; set; } = "SBIN0001234";
    public string InvoicePrefix { get; set; } = "RST-";
    public decimal DefaultGstRate { get; set; } = 5.00m;
    public string? TermsAndConditions { get; set; } = "1. Goods exchanged within 7 days with tags. 2. No cash refund.";
}

[Table("Users")]
public class User
{
    [Key]
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PinHash { get; set; } = "1234";
    public string UserRole { get; set; } = "CASHIER";
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Products")]
public class Product
{
    [Key]
    public int ProductId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Fabric { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string HsnCode { get; set; } = "5208";
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal GstRate { get; set; } = 5.0m;
    public int StockQuantity { get; set; }
    public int MinStockAlert { get; set; } = 5;
    public string RackLocation { get; set; } = "A-01";
    public string Unit { get; set; } = "Pcs";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Customers")]
public class Customer
{
    [Key]
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AddressLine { get; set; }
    public string? CustomerGSTIN { get; set; }
    public int LoyaltyPoints { get; set; } = 0;
    public decimal CreditBalance { get; set; } = 0.00m;
    public decimal TotalPurchases { get; set; } = 0.00m;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Suppliers")]
public class Supplier
{
    [Key]
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AddressLine { get; set; }
    public string GSTIN { get; set; } = string.Empty;
    public string StateName { get; set; } = "Tamil Nadu";
    public decimal CreditBalance { get; set; } = 0.00m;
    public string PaymentTerms { get; set; } = "30 Days Net";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("DiscountOffers")]
public class DiscountOffer
{
    [Key]
    public int OfferId { get; set; }
    public string OfferCode { get; set; } = string.Empty;
    public string OfferName { get; set; } = string.Empty;
    public string DiscountType { get; set; } = "PERCENT";
    public decimal DiscountValue { get; set; }
    public decimal MinBillAmount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime ValidTill { get; set; } = DateTime.UtcNow.AddMonths(3);
    public int UsageCount { get; set; } = 0;
}

[Table("Expenses")]
public class Expense
{
    [Key]
    public int ExpenseId { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public decimal Amount { get; set; }
    public string PaymentMode { get; set; } = "CASH";
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public string Recipient { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string RecordedBy { get; set; } = "Admin";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("StockAdjustments")]
public class StockAdjustment
{
    [Key]
    public int AdjustmentId { get; set; }
    public int ProductId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public int ChangeQuantity { get; set; }
    public string Reason { get; set; } = "Stock Audit";
    public string RecordedBy { get; set; } = "Admin";
    public string? Notes { get; set; }
    public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;
}

[Table("Purchases")]
public class Purchase
{
    [Key]
    public int PurchaseId { get; set; }
    public string PurchaseNumber { get; set; } = string.Empty;
    public string SupplierInvoiceNo { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public decimal TotalTaxable { get; set; }
    public decimal TotalGst { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public string PaymentStatus { get; set; } = "PAID";
    public string ReceivedBy { get; set; } = "Admin";
    public string? Notes { get; set; }
    public List<PurchaseItem> Items { get; set; } = new();
}

[Table("PurchaseItems")]
public class PurchaseItem
{
    [Key]
    public int PurchaseItemId { get; set; }
    public int PurchaseId { get; set; }
    public int ProductId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal GstRate { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal GstAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

[Table("SalesReturns")]
public class SalesReturn
{
    [Key]
    public int ReturnId { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string OriginalInvoiceNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public decimal ReturnedValue { get; set; }
    public decimal ExchangeValue { get; set; }
    public decimal NetRefundAmount { get; set; }
    public string PaymentMode { get; set; } = "CASH";
    public string ProcessedBy { get; set; } = "Admin";
    public string? Notes { get; set; }
}

[Table("Invoices")]
public class Invoice
{
    [Key]
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public int? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerAddress { get; set; }
    public string? CustomerGstin { get; set; }
    public bool IsInterState { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ItemDiscountTotal { get; set; }
    public decimal BillDiscountAmount { get; set; }
    public string? BillDiscountCode { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal CgstTotal { get; set; }
    public decimal SgstTotal { get; set; }
    public decimal IgstTotal { get; set; }
    public decimal TotalTax { get; set; }
    public decimal RoundOff { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }
    public string PaymentStatus { get; set; } = "PAID";
    public int CashierId { get; set; } = 1;
    public string CashierName { get; set; } = "Admin";
    public string? Notes { get; set; }

    public List<InvoiceItem> Items { get; set; } = new();
    public List<InvoicePayment> Payments { get; set; } = new();
}

[Table("InvoiceItems")]
public class InvoiceItem
{
    [Key]
    public int InvoiceItemId { get; set; }
    public int InvoiceId { get; set; }
    public int ProductId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string HsnCode { get; set; } = string.Empty;
    public string Unit { get; set; } = "Pcs";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal GstRate { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal LineTotal { get; set; }
}

[Table("InvoicePayments")]
public class InvoicePayment
{
    [Key]
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public string PaymentMode { get; set; } = "CASH";
    public decimal Amount { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}
