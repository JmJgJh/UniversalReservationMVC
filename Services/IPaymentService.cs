using UniversalReservationMVC.Models;

namespace UniversalReservationMVC.Services;

public interface IPaymentService
{
    Task<Payment> CreatePaymentIntentAsync(Reservation reservation, decimal amount, string currency = "PLN");
    Task<Payment> ConfirmPaymentAsync(string paymentIntentId);
    Task<Payment> RefundPaymentAsync(int paymentId, string reason);
    Task<Payment?> GetPaymentByReservationIdAsync(int reservationId);
    Task<Payment?> GetPaymentByIntentIdAsync(string paymentIntentId);
    Task HandleWebhookEventAsync(string json, string signature);
}
