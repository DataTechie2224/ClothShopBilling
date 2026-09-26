using System.Collections.Generic;

namespace ClothShop.Api.DTOs;

public class CartItemDto
{
    public int ProductId { get; set; }
    public string? Barcode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Brand { get; set; } = string.Empty;
    public string? Category { get; set; } = string.Empty;
    public string? Size { get; set; } = string.Empty;
    public string? Color { get; set; } = string.Empty;
    public string? HsnCode { get; set; } = "5208";
    public string? Unit { get; set; } = "Pcs";
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal GstRate { get; set; } = 5;
}

public class PaymentRecordDto
{
    public string Mode { get; set; } = "CASH";
    public decimal Amount { get; set; }
    public string? ReferenceNo { get; set; }
}

public class CheckoutRequestDto
{
    public int? CustomerId { get; set; }
    public string CustomerName { get; set; } = "Walk-in Customer";
    public string CustomerPhone { get; set; } = "9999999999";
    public string? CustomerAddress { get; set; }
    public string? CustomerGstin { get; set; }
    public bool IsInterState { get; set; } = false;
    public decimal BillDiscountAmount { get; set; } = 0.0m;
    public string? BillDiscountCode { get; set; }
    public decimal AmountPaid { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public List<PaymentRecordDto> Payments { get; set; } = new();
    public int CashierId { get; set; } = 1;
    public string CashierName { get; set; } = "Admin";
    public string? Notes { get; set; }
}