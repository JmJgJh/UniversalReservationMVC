using Microsoft.EntityFrameworkCore;
using Stripe;
using UniversalReservationMVC.Data;
using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentService> _logger;
    private readonly IConfiguration _configuration;

    public PaymentService(
        ApplicationDbContext context,
        ILogger<PaymentService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;

        // Initialize Stripe API key
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<Models.Payment> CreatePaymentIntentAsync(
        Reservation reservation,
        decimal amount,
        string currency = "PLN")
    {
        try
        {
            // Create Stripe Payment Intent
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents/grosze
                Currency = currency.ToLower(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = new Dictionary<string, string>
                {
                    { "reservation_id", reservation.Id.ToString() },
                    { "user_id", reservation.UserId ?? "guest" },
                    { "guest_email", reservation.GuestEmail ?? "" }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            // Create Payment record
            var payment = new Models.Payment
            {
                ReservationId = reservation.Id,
                StripePaymentIntentId = paymentIntent.Id,
                Amount = amount,
                Currency = currency,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Metadata = System.Text.Json.JsonSerializer.Serialize(new
                {
                    client_secret = paymentIntent.ClientSecret,
                    status = paymentIntent.Status
                })
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Created payment intent {PaymentIntentId} for reservation {ReservationId}",
                paymentIntent.Id,
                reservation.Id);

            return payment;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating payment intent for reservation {ReservationId}", reservation.Id);
            throw new InvalidOperationException($"Błąd tworzenia płatności: {ex.Message}", ex);
        }
    }

    public async Task<Models.Payment> ConfirmPaymentAsync(string paymentIntentId)
    {
        var payment = await GetPaymentByIntentIdAsync(paymentIntentId);
        if (payment == null)
        {
            throw new InvalidOperationException($"Nie znaleziono płatności o ID: {paymentIntentId}");
        }

        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            if (paymentIntent.Status == "succeeded")
            {
                payment.Status = PaymentStatus.Succeeded;
                payment.PaidAt = DateTime.UtcNow;
                payment.StripeChargeId = paymentIntent.LatestChargeId;

                // Update reservation status
                var reservation = await _context.Reservations.FindAsync(payment.ReservationId);
                if (reservation != null)
                {
                    reservation.IsPaid = true;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment {PaymentIntentId} confirmed successfully", paymentIntentId);
            }

            return payment;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error confirming payment {PaymentIntentId}", paymentIntentId);
            throw new InvalidOperationException($"Błąd potwierdzania płatności: {ex.Message}", ex);
        }
    }

    public async Task<Models.Payment> RefundPaymentAsync(int paymentId, string reason)
    {
        var payment = await _context.Payments
            .Include(p => p.Reservation)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
        {
            throw new InvalidOperationException($"Nie znaleziono płatności o ID: {paymentId}");
        }

        if (payment.Status != PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException("Można zwrócić tylko pomyślnie opłacone płatności");
        }

        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = payment.StripePaymentIntentId,
                Reason = "requested_by_customer"
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options);

            payment.Status = PaymentStatus.Refunded;
            payment.FailureReason = reason;

            // Update reservation
            if (payment.Reservation != null)
            {
                payment.Reservation.IsPaid = false;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Payment {PaymentId} refunded successfully. Reason: {Reason}",
                paymentId,
                reason);

            return payment;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error refunding payment {PaymentId}", paymentId);
            throw new InvalidOperationException($"Błąd zwrotu płatności: {ex.Message}", ex);
        }
    }

    public async Task<Models.Payment?> GetPaymentByReservationIdAsync(int reservationId)
    {
        return await _context.Payments
            .Include(p => p.Reservation)
            .FirstOrDefaultAsync(p => p.ReservationId == reservationId);
    }

    public async Task<Models.Payment?> GetPaymentByIntentIdAsync(string paymentIntentId)
    {
        return await _context.Payments
            .Include(p => p.Reservation)
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);
    }

    public async Task HandleWebhookEventAsync(string json, string signature)
    {
        try
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signature,
                webhookSecret);

            _logger.LogInformation("Processing Stripe webhook event: {EventType}", stripeEvent.Type);

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null)
                {
                    await ConfirmPaymentAsync(paymentIntent.Id);
                }
            }
            else if (stripeEvent.Type == "payment_intent.payment_failed")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null)
                {
                    var payment = await GetPaymentByIntentIdAsync(paymentIntent.Id);
                    if (payment != null)
                    {
                        payment.Status = PaymentStatus.Failed;
                        payment.FailureReason = paymentIntent.LastPaymentError?.Message ?? "Payment failed";
                        await _context.SaveChangesAsync();

                        _logger.LogWarning(
                            "Payment {PaymentIntentId} failed: {Reason}",
                            paymentIntent.Id,
                            payment.FailureReason);
                    }
                }
            }
            else if (stripeEvent.Type == "charge.refunded")
            {
                var charge = stripeEvent.Data.Object as Charge;
                if (charge != null && charge.PaymentIntentId != null)
                {
                    var payment = await GetPaymentByIntentIdAsync(charge.PaymentIntentId);
                    if (payment != null && payment.Status != PaymentStatus.Refunded)
                    {
                        payment.Status = PaymentStatus.Refunded;
                        payment.FailureReason = "Zwrot środków przez Stripe";

                        if (payment.Reservation != null)
                        {
                            payment.Reservation.IsPaid = false;
                        }

                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Payment {PaymentId} marked as refunded via webhook", payment.Id);
                    }
                }
            }
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            throw;
        }
    }
}
