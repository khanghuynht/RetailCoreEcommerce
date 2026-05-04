using System.ComponentModel.DataAnnotations;
using RetailCoreEcommerce.Contracts.Models.Checkout;

namespace RetailCoreEcommerce.StoreFront.Models;

public class PaymentViewModel
{
    // ── Shipping fields ──────────────────────────────────────────────────────

    [Required(ErrorMessage = "Recipient name is required.")]
    [Display(Name = "Full Name")]
    public string RecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Street address is required.")]
    [Display(Name = "Street Address")]
    public string StreetAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Province is required.")]
    public string Province { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ward is required.")]
    public string Ward { get; set; } = string.Empty;

    public string? Notes { get; set; }

    // ── Display data (populated by controller, not submitted) ────────────────

    public PreviewCheckoutResponse? Preview { get; set; }
}
