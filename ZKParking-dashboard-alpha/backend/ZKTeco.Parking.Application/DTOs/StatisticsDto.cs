namespace ZKTeco.Parking.Application.DTOs;

public class RevenueChartDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int TicketCount { get; set; }
    public DateTime Date { get; set; }
}

public class OccupancyChartDto
{
    public int Hour { get; set; }
    public string HourLabel { get; set; } = string.Empty;
    public int Entries { get; set; }
    public int Exits { get; set; }
    public int Occupancy { get; set; }
}

public class TicketTypeDistributionDto
{
    public string TicketType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class AverageDurationDto
{
    public string DayLabel { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double AverageDurationMinutes { get; set; }
    public int TicketCount { get; set; }
}

public class MonthlyRevenueDto
{
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int TicketCount { get; set; }
    public int Year { get; set; }
}
