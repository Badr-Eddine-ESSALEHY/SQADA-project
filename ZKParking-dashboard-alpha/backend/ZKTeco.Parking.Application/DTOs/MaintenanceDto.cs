namespace ZKTeco.Parking.Application.DTOs;

public class GateDto
{
    public int Id { get; set; }
    public int ParkingId { get; set; }
    public string ParkingName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastPing { get; set; }
    public bool IsActive { get; set; }
}

public class TerminalDto
{
    public int Id { get; set; }
    public int ParkingId { get; set; }
    public string ParkingName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastPing { get; set; }
    public string? FirmwareVersion { get; set; }
    public bool IsActive { get; set; }
}

public class AlertDto
{
    public int Id { get; set; }
    public int ParkingId { get; set; }
    public string ParkingName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class ParkingDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int TotalSpaces { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal DailyRate { get; set; }
    public decimal MonthlyRate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
