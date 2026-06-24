namespace ZKTeco.Parking.Application.DTOs;

public class DashboardStatsDto
{
    public int TodayEntries { get; set; }
    public int TodayExits { get; set; }
    public int CurrentOccupancy { get; set; }
    public int AvailableSpaces { get; set; }
    public int TotalSpaces { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public int LostTickets { get; set; }
    public int UnreadableTickets { get; set; }
    public int ActiveSubscribers { get; set; }
    public double AverageDuration { get; set; }
    public decimal AverageTicketAmount { get; set; }
    public int ExpiringSubscriptions { get; set; }
}
