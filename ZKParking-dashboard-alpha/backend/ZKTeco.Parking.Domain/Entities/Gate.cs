namespace ZKTeco.Parking.Domain.Entities;

public class Gate
{
    public int Id { get; set; }
    public int ParkingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Entry / Exit
    public string? IpAddress { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastPing { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Parking? Parking { get; set; }
    public ICollection<ParkingRecord> EntryRecords { get; set; } = new List<ParkingRecord>();
    public ICollection<ParkingRecord> ExitRecords { get; set; } = new List<ParkingRecord>();
}
