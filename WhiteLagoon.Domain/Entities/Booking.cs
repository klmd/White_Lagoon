using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhiteLagoon.Domain.Entities;

public class Booking
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string UserId { get; set; }
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }
    
    [Required]
    public int VillaId { get; set; }
    [ForeignKey("VillaId")]
    public Villa Villa { get; set; }
    
    [Required]
    public string Name { get; set; }
    [Required]
    public string Email { get; set; }
    public string? Phone { get; set; }

    [Required]
    public double TotalCost { get; set; }
    public int Nights { get; set; }
    public string? Status { get; set; }

    [Required]
    public DateOnly BookingDate { get; set; }
    [Required]
    public DateOnly CheckInDate { get; set; }
    [Required]
    public DateOnly CheckOutDate { get; set; }
    
    public bool IsPaymentSuccessful { get; set; }
    public DateOnly PaymentDate { get; set; }

    public string? StripeSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }

    public DateOnly ActualCheckInDate { get; set; }
    public DateOnly ActualCheckOutDate { get; set; }

    public int VillaNumber { get; set; }
}