using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalReservationMVC.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [ForeignKey("ReservationId")]
        public virtual Reservation? Reservation { get; set; }

        [Required]
        [StringLength(100)]
        public string StripePaymentIntentId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? StripeChargeId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "PLN";

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        [StringLength(500)]
        public string? FailureReason { get; set; }

        [StringLength(1000)]
        public string? Metadata { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Succeeded,
        Failed,
        Refunded,
        Cancelled
    }
}
