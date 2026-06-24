namespace ZKTeco.Parking.Domain.Entities;

public class Alert
{
    public int Id { get; set; }
    public int ParkingId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public Parking? Parking { get; set; }
}
