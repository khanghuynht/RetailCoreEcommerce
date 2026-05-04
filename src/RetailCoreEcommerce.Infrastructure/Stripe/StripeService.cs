using Microsoft.Extensions.Options;
using RetailCoreEcommerce.Application.Abstractions;
using RetailCoreEcommerce.Contracts.Settings;
using Stripe;

namespace RetailCoreEcommerce.Infrastructure.Stripe;

public class StripeService : IStripeService
{
    private readonly StripeSettings _settings;

    public string PublishableKey => _settings.PublishableKey;

    public StripeService(IOptions<StripeSettings> settings)
    {
        _settings = settings.Value;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<(string PaymentIntentId, string ClientSecret)> CreatePaymentIntentAsync(
        decimal amount, string currency, Guid orderId, CancellationToken ct = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            // Stripe expects amount in smallest currency unit (cents)
            Amount = (long)(amount * 100),
            Currency = currency.ToLower(),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            },
            Metadata = new Dictionary<string, string>
            {
                ["orderId"] = orderId.ToString()
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options, cancellationToken: ct);

        return (intent.Id, intent.ClientSecret);
    }

    public (bool IsValid, string EventType, string? PaymentIntentId) ParseWebhookEvent(
        string json, string stripeSignature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                _settings.WebhookSecret,
                throwOnApiVersionMismatch: false);

            string? paymentIntentId = null;

            if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
                paymentIntentId = paymentIntent.Id;

            return (true, stripeEvent.Type, paymentIntentId);
        }
        catch
        {
            return (false, string.Empty, null);
        }
    }
}
