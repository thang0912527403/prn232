using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EbayClone.API.Models;
public class EmailMessage
{
    public string? To { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
}
public class Product
{
    public string ProductId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? CategoryId { get; set; }
    public string? ImageUrl { get; set; }

    [Required]
    public string SellerId { get; set; } = string.Empty;

    public bool IsAuction { get; set; } = true;
    public DateTime? AuctionEndTime { get; set; }

    [ForeignKey(nameof(SellerId))]
    public virtual User? Seller { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
public class Category
{
    public string CategoryId { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string Name { get; set; } = string.Empty;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
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
    [JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    [JsonIgnore]

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    [JsonIgnore]

    public virtual ICollection<PaymentInfo> Payments { get; set; } = new List<PaymentInfo>();
    [JsonIgnore]

    public virtual ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();
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

public class OrderItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string? OrderId { get; set; } = string.Empty;

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    [Required]
    public string ProductId { get; set; } = string.Empty;

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }

    public decimal Price { get; set; }
    public int Quantity { get; set; }

    [NotMapped]
    public decimal Subtotal => Price * Quantity;
}

public class PaymentInfo
{
    public string PaymentInfoId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string OrderId { get; set; } = string.Empty;

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    public string TransactionId { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "PayPal";
    public string? PayPalOrderId { get; set; }
    public string? PayPalCaptureId { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }

    [MaxLength(100)]
    public string? PayPalPayerId { get; set; }

    [MaxLength(100)]
    public string? PayPalPaymentId { get; set; }
}

public class ShippingInfo
{
    public string ShippingInfoId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string OrderId { get; set; } = string.Empty;

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    public string TrackingNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public ShippingStatus Status { get; set; }

    public DateTime? ShippedDate { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public DateTime? ActualDelivery { get; set; }

    public List<ShippingEvent> Events { get; set; } = new();
}

public class ShippingEvent
{
    public string ShippingEventId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string ShippingInfoId { get; set; } = string.Empty;

    [ForeignKey(nameof(ShippingInfoId))]
    public virtual ShippingInfo? ShippingInfo { get; set; }

    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class EscrowInfo
{
    public string EscrowInfoId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string OrderId { get; set; } = string.Empty;

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    public decimal Amount { get; set; }
    public EscrowStatus Status { get; set; } = EscrowStatus.Held;
    public DateTime? HeldDate { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public int EscrowPeriodDays { get; set; } = 21;
    public DateTime? ReleasedAt { get; set; }
    public string? RefundReason { get; set; }
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

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded
}

public enum ShippingStatus
{
    NotShipped,
    Processing,
    Shipped,
    InTransit,
    OutForDelivery,
    Delivered,
    Failed,
    Returned
}

public enum EscrowStatus
{
    Held,
    Released,
    Refunded
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
public class CreateOrderRequest
{
    public string UserId { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public decimal SellerRating { get; set; } = 4.0m;
    public List<OrderItem> Items { get; set; } = new();
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingRegion { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
}

public class PaymentRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string PayPalOrderId { get; set; } = string.Empty;
}

public class UpdateShippingRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
}

public class DisputeRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
public class ShippingRegion
{
    public int ShippingRegionId { get; set; }
    public string Name { get; set; }
    public decimal Cost { get; set; }
}