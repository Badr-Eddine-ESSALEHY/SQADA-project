using ZKTeco.Parking.Application.DTOs;
using ZKTeco.Parking.Application.Interfaces;
using ZKTeco.Parking.Domain.Interfaces;

namespace ZKTeco.Parking.Application.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IParkingRecordRepository _repository;

    public StatisticsService(IParkingRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<RevenueChartDto>> GetDailyRevenueAsync(int parkingId, DateTime startDate, DateTime endDate)
    {
        var records = await _repository.GetByDateRangeAsync(parkingId, startDate, endDate);
        var recordsList = records.ToList();

        var result = new List<RevenueChartDto>();
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayRecords = recordsList.Where(r => r.EntryTime.Date == date).ToList();
            result.Add(new RevenueChartDto
            {
                Label = date.ToString("dd/MM"),
                Date = date,
                Revenue = dayRecords.Where(r => r.Amount.HasValue).Sum(r => r.Amount!.Value),
                TicketCount = dayRecords.Count
            });
        }

        return result;
    }

    public async Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int parkingId, int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31, 23, 59, 59);
        var records = await _repository.GetByDateRangeAsync(parkingId, startDate, endDate);
        var recordsList = records.ToList();

        var result = new List<MonthlyRevenueDto>();
        for (int month = 1; month <= 12; month++)
        {
            var monthRecords = recordsList.Where(r => r.EntryTime.Month == month).ToList();
            result.Add(new MonthlyRevenueDto
            {
                Month = month,
                MonthLabel = new DateTime(year, month, 1).ToString("MMM"),
                Year = year,
                Revenue = monthRecords.Where(r => r.Amount.HasValue).Sum(r => r.Amount!.Value),
                TicketCount = monthRecords.Count
            });
        }

        return result;
    }

    public async Task<IEnumerable<OccupancyChartDto>> GetOccupancyByHourAsync(int parkingId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);
        var records = await _repository.GetByDateRangeAsync(parkingId, startOfDay, endOfDay);
        var recordsList = records.ToList();

        var result = new List<OccupancyChartDto>();
        int runningOccupancy = 0;

        for (int hour = 0; hour < 24; hour++)
        {
            var hourStart = startOfDay.AddHours(hour);
            var hourEnd = hourStart.AddHours(1);

            var entries = recordsList.Count(r => r.EntryTime >= hourStart && r.EntryTime < hourEnd);
            var exits = recordsList.Count(r => r.ExitTime.HasValue && r.ExitTime >= hourStart && r.ExitTime < hourEnd);

            runningOccupancy = Math.Max(0, runningOccupancy + entries - exits);

            result.Add(new OccupancyChartDto
            {
                Hour = hour,
                HourLabel = $"{hour:D2}:00",
                Entries = entries,
                Exits = exits,
                Occupancy = runningOccupancy
            });
        }

        return result;
    }

    public async Task<IEnumerable<TicketTypeDistributionDto>> GetTicketTypeDistributionAsync(int parkingId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);
        var records = await _repository.GetByDateRangeAsync(parkingId, startOfDay, endOfDay);
        var recordsList = records.ToList();

        var totalCount = recordsList.Count;
        var grouped = recordsList
            .GroupBy(r => r.TicketType)
            .Select(g => new TicketTypeDistributionDto
            {
                TicketType = g.Key,
                Count = g.Count(),
                Percentage = totalCount > 0 ? Math.Round((decimal)g.Count() / totalCount * 100, 2) : 0,
                TotalRevenue = g.Where(r => r.Amount.HasValue).Sum(r => r.Amount!.Value)
            });

        return grouped;
    }

    public async Task<IEnumerable<AverageDurationDto>> GetAverageDurationByDayAsync(int parkingId, int days)
    {
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-days + 1);
        var records = await _repository.GetByDateRangeAsync(parkingId, startDate, endDate.AddDays(1).AddTicks(-1));
        var recordsList = records.Where(r => r.Duration.HasValue).ToList();

        var result = new List<AverageDurationDto>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dayRecords = recordsList.Where(r => r.EntryTime.Date == date).ToList();
            result.Add(new AverageDurationDto
            {
                DayLabel = date.ToString("dd/MM"),
                Date = date,
                AverageDurationMinutes = dayRecords.Any() ? Math.Round(dayRecords.Average(r => r.Duration!.Value.TotalMinutes), 2) : 0,
                TicketCount = dayRecords.Count
            });
        }

        return result;
    }
}
