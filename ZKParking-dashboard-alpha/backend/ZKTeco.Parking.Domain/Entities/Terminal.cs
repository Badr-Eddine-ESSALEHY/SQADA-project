namespace ZKTeco.Parking.Domain.Entities;

public class Terminal
{
    public int Id { get; set; }
    public int ParkingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastPing { get; set; }
    public string? FirmwareVersion { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Parking? Parking { get; set; }
}
