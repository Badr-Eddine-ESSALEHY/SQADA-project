using ZKTeco.Parking.Application.DTOs;

namespace ZKTeco.Parking.Application.Interfaces;

public interface IStatisticsService
{
    Task<IEnumerable<RevenueChartDto>> GetDailyRevenueAsync(int parkingId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int parkingId, int year);
    Task<IEnumerable<OccupancyChartDto>> GetOccupancyByHourAsync(int parkingId, DateTime date);
    Task<IEnumerable<TicketTypeDistributionDto>> GetTicketTypeDistributionAsync(int parkingId, DateTime date);
    Task<IEnumerable<AverageDurationDto>> GetAverageDurationByDayAsync(int parkingId, int days);
}
