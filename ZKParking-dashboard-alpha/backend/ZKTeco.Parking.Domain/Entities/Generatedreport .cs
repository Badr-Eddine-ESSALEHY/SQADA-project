namespace ZKTeco.Parking.Domain.Entities;

public class GeneratedReport
{
    public int Id { get; set; }
    public int ParkingId { get; set; }
    public string ParkingName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;   // "daily" | "weekly" | "monthly" | "annual" | "custom"
    public string Title { get; set; } = string.Empty;        // display title, e.g. "Rapport Journalier"
    public string PeriodLabel { get; set; } = string.Empty;  // display period, e.g. "29/06/2026"
    public string Format { get; set; } = string.Empty;       // "PDF" | "EXCEL"
    public long FileSizeBytes { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}