using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EbayClone.Web.Models;

// These should match the API models
public class User
{
    public string UserId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public string? Role { get; set; }
    public SellerTrustLevel TrustLevel { get; set; } = SellerTrustLevel.New;
    public decimal Rating { get; set; } = 4.5m;
    public int TotalSales { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<PaymentInfo> Payments { get; set; } = new List<PaymentInfo>();
    public virtual ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();
}
public enum SellerTrustLevel
{
    New = 21,        // 21 ngày giữ tiền
    Bronze = 14,     // 14 ngày
    Silver = 7,      // 7 ngày
    Gold = 3,        // 3 ngày
    Platinum = 1,     // 1 ngày
    Diamond = 0      // Không giữ tiền
}
public class ReturnRequest
{
    public string ReturnRequestId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string OrderId { get; set; } = string.Empty;

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    public string Reason { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public bool IsApproved { get; set; }
    public string? Comments { get; set; }
}

public class Order
{
    public string OrderId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; } = string.Empty;


    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    [Required]
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingRegion { get; set; } = string.Empty;
    public decimal ShippingFee { get; set; }
    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;
    public string? PaymentInfoId { get; set; }
    public string? ShippingInfoId { get; set; }
    public string? EscrowInfoId { get; set; }

    [ForeignKey(nameof(PaymentInfoId))]
    public PaymentInfo? Payment { get; set; }

    [ForeignKey(nameof(ShippingInfoId))]
    public ShippingInfo? Shipping { get; set; }

    [ForeignKey(nameof(EscrowInfoId))]
    public EscrowInfo? Escrow { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? ReasonForCancellation { get; set; }
    public virtual ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();
}
public enum OrderStatus
{
    PendingPayment,
    PaymentFailed,
    Paid,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded,
    Disputed,
    Returned,
    ReturnRequested
}
public class OrderItem
{
    public string? orderId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    //public string? ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    //public decimal Subtotal => Price * Quantity;
}

public class PaymentInfo
{
    public string TransactionId { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "PayPal";
    public string PayPalOrderId { get; set; } = string.Empty;
    public string PayPalCaptureId { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime? ProcessedAt { get; set; }
}

public class ShippingInfo
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string Status { get; set; } = "NotShipped";
    public DateTime? ShippedDate { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public DateTime? ActualDelivery { get; set; }
    public List<ShippingEvent> Events { get; set; } = new();
}

public class ShippingEvent
{
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class EscrowInfo
{
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Held";
    public DateTime HeldDate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public int EscrowPeriodDays { get; set; }
    public DateTime? ReleasedAt { get; set; }
}

public class CreateOrderViewModel
{
    public string UserId { get; set; } = "buyer@example.com";
    public string SellerId { get; set; } = "seller@example.com";
    public decimal SellerRating { get; set; } = 4.5m;
    public List<OrderItem> Items { get; set; } = new();
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingRegion { get; set; } = "Hanoi";
    public string? CouponCode { get; set; }
}

public class Product
{
    public string productId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public decimal SellerRating { get; set; } = 4.0m;
    public User? Seller { get; set; }
}
public class Category
{
    public string categoryId { get; set; }
    public string Name { get; set; } = string.Empty;
}
public class ShippingRegion
{
    public int ShippingRegionId { get; set; }
    public string Name { get; set; }
    public decimal Cost { get; set; }
}