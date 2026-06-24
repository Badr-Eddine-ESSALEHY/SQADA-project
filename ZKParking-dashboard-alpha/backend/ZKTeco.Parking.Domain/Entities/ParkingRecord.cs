namespace ZKTeco.Parking.Domain.Entities;

public class ParkingRecord
{
    public int Id { get; set; }
    public string CardNo { get; set; } = string.Empty;
    public string PlateNo { get; set; } = string.Empty;
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public decimal? Amount { get; set; }
    public string TicketType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ParkingId { get; set; }
    public int? OperatorId { get; set; }
    public int? GateInId { get; set; }
    public int? GateOutId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Parking? Parking { get; set; }
    public Operator? Operator { get; set; }
    public Gate? GateIn { get; set; }
    public Gate? GateOut { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
