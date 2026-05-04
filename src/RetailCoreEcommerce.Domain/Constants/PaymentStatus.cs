namespace RetailCoreEcommerce.Domain.Constants;

public enum PaymentStatus
{
    Pending,        // PaymentIntent created, customer hasn't paid yet
    Processing,     // Payment is being processed
    Succeeded,      // stripe: payment_intent.succeeded
    Failed,         // stripe: payment_intent.payment_failed
    Cancelled,      // order cancelled before payment
    Refunded        // stripe: charge.refunded
}