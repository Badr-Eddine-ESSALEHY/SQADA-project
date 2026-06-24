namespace ZKTeco.Parking.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int ParkingRecordId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public int? OperatorId { get; set; }
    public string Status { get; set; } = string.Empty;

    // Navigation properties
    public ParkingRecord? ParkingRecord { get; set; }
    public Operator? Operator { get; set; }
}
