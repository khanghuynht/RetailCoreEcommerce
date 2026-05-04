using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailCoreEcommerce.Application.Abstractions;

namespace RetailCoreEcommerce.API.Controllers;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/webhook")]
[AllowAnonymous]
public class StripeWebhookController : ControllerBase
{
    private readonly IStripeService _stripeService;
    private readonly IOrderService _orderService;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        IStripeService stripeService,
        IOrderService orderService,
        ILogger<StripeWebhookController> logger)
    {
        _stripeService = stripeService;
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        // Read raw body — must NOT use [FromBody] or model binding
        // because Stripe verifies the exact bytes for the signature
        string json;
        using (var reader = new StreamReader(HttpContext.Request.Body))
            json = await reader.ReadToEndAsync(cancellationToken);

        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Stripe webhook received without Stripe-Signature header.");
            return BadRequest("Missing Stripe-Signature header.");
        }

        var (isValid, eventType, paymentIntentId) = _stripeService.ParseWebhookEvent(json, signature);

        if (!isValid)
        {
            _logger.LogWarning("Invalid Stripe webhook signature.");
            return BadRequest("Invalid webhook signature.");
        }

        _logger.LogInformation("Stripe webhook received: {EventType}", eventType);

        switch (eventType)
        {
            case "payment_intent.succeeded":
                if (paymentIntentId is not null)
                    await _orderService.HandlePaymentSucceededAsync(paymentIntentId, cancellationToken);
                break;

            case "payment_intent.payment_failed":
                if (paymentIntentId is not null)
                    await _orderService.HandlePaymentFailedAsync(paymentIntentId, cancellationToken);
                break;

            default:
                _logger.LogDebug("Unhandled Stripe event type: {EventType}", eventType);
                break;
        }

        // Always return 200 — Stripe retries on non-2xx
        return Ok();
    }
}
