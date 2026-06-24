namespace ZKTeco.Parking.Domain.Entities;

public class Parking
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int TotalSpaces { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal DailyRate { get; set; }
    public decimal MonthlyRate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ParkingRecord> ParkingRecords { get; set; } = new List<ParkingRecord>();
    public ICollection<Subscriber> Subscribers { get; set; } = new List<Subscriber>();
    public ICollection<Gate> Gates { get; set; } = new List<Gate>();
    public ICollection<Terminal> Terminals { get; set; } = new List<Terminal>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
