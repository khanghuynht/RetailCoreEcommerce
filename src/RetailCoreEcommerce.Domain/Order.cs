using System.ComponentModel.DataAnnotations;
using RetailCoreEcommerce.Domain.Abstractions;
using RetailCoreEcommerce.Domain.Constants;

namespace RetailCoreEcommerce.Domain;

public class Order : AuditableEntity<Guid>
{
    public Guid UserId { get; set; }
    public required string OrderCode { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public required OrderStatus Status { get; set; }
    public required string RecipientName { get; set; } = null!;
    public required string Email { get; set; } = null!;
    public required string PhoneNumber { get; set; } = null!;
    public required string StreetAddress { get; set; } = null!;
    public required string Province { get; set; } = null!;
    public required string Ward { get; set; } = null!;
    public decimal? ShippingFee { get; set; }
    public string Notes { get; set; } = null!;
    public string? StripePaymentIntentId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = [];
    public virtual ICollection<OrderHistory> OrderHistories { get; set; } = [];
}