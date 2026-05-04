namespace RetailCoreEcommerce.Application.Abstractions;

public interface IStripeService
{
    string PublishableKey { get; }

    /// <summary>Creates a Stripe PaymentIntent and returns (paymentIntentId, clientSecret).</summary>
    Task<(string PaymentIntentId, string ClientSecret)> CreatePaymentIntentAsync(
        decimal amount, string currency, Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Parses and verifies a Stripe webhook payload.
    /// Returns (IsValid, EventType, PaymentIntentId).
    /// PaymentIntentId is populated for payment_intent.* events.
    /// </summary>
    (bool IsValid, string EventType, string? PaymentIntentId) ParseWebhookEvent(
        string json, string stripeSignature);
}
