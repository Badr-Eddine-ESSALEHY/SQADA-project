using ZKTeco.Parking.Application.DTOs;
using ZKTeco.Parking.Application.Interfaces;
using ZKTeco.Parking.Domain.Interfaces;

namespace ZKTeco.Parking.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IParkingRecordRepository _parkingRecordRepository;
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly IParkingRepository _parkingRepository;

    public DashboardService(
        IParkingRecordRepository parkingRecordRepository,
        ISubscriberRepository subscriberRepository,
        IParkingRepository parkingRepository)
    {
        _parkingRecordRepository = parkingRecordRepository;
        _subscriberRepository = subscriberRepository;
        _parkingRepository = parkingRepository;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(int parkingId, DateTime date)
    {
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);
        var startOfMonth = new DateTime(date.Year, date.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

        var todayRecords = await _parkingRecordRepository.GetByDateRangeAsync(parkingId, startOfDay, endOfDay);
        var recordsList = todayRecords.ToList();

        var todayEntries = recordsList.Count;
        var todayExits = recordsList.Count(r => r.ExitTime.HasValue);
        var currentOccupancy = await _parkingRecordRepository.GetCurrentOccupancyAsync(parkingId);
        var totalSpaces = parking?.TotalSpaces ?? 100;
        var availableSpaces = Math.Max(0, totalSpaces - currentOccupancy);
        var occupancyRate = totalSpaces > 0 ? Math.Round((decimal)currentOccupancy / totalSpaces * 100, 2) : 0;

        var todayRevenue = await _parkingRecordRepository.GetTotalRevenueAsync(parkingId, startOfDay, endOfDay);
        var monthRevenue = await _parkingRecordRepository.GetTotalRevenueAsync(parkingId, startOfMonth, endOfMonth);

        var lostTickets = recordsList.Count(r => r.TicketType == "Lost");
        var unreadableTickets = recordsList.Count(r => r.TicketType == "Unreadable");

        var activeSubscribers = await _subscriberRepository.GetActiveCountAsync(parkingId);
        var expiringSubscriptions = (await _subscriberRepository.GetExpiringSubscribersAsync(parkingId, 7)).Count();

        var completedRecords = recordsList.Where(r => r.Duration.HasValue).ToList();
        var averageDuration = completedRecords.Any()
            ? completedRecords.Average(r => r.Duration!.Value.TotalMinutes)
            : 0;

        var paidRecords = recordsList.Where(r => r.Amount.HasValue && r.Amount > 0).ToList();
        var averageTicketAmount = paidRecords.Any()
            ? paidRecords.Average(r => r.Amount!.Value)
            : 0;

        return new DashboardStatsDto
        {
            TodayEntries = todayEntries,
            TodayExits = todayExits,
            CurrentOccupancy = currentOccupancy,
            AvailableSpaces = availableSpaces,
            TotalSpaces = totalSpaces,
            OccupancyRate = occupancyRate,
            TodayRevenue = todayRevenue,
            MonthRevenue = monthRevenue,
            LostTickets = lostTickets,
            UnreadableTickets = unreadableTickets,
            ActiveSubscribers = activeSubscribers,
            AverageDuration = Math.Round(averageDuration, 2),
            AverageTicketAmount = Math.Round(averageTicketAmount, 2),
            ExpiringSubscriptions = expiringSubscriptions
        };
    }

    public async Task<IEnumerable<ParkingDto>> GetParkingListAsync()
    {
        var parkings = await _parkingRepository.GetActiveParkingsAsync();
        return parkings.Select(p => new ParkingDto
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            Address = p.Address,
            TotalSpaces = p.TotalSpaces,
            HourlyRate = p.HourlyRate,
            DailyRate = p.DailyRate,
            MonthlyRate = p.MonthlyRate,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<IEnumerable<RevenueChartDto>> GetRevenueChartAsync(int parkingId, int days)
    {
        var result = new List<RevenueChartDto>();
        var today = DateTime.Today;
        for (int i = days - 1; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            var records = (await _parkingRecordRepository.GetByDateRangeAsync(
                parkingId, day, day.AddDays(1).AddTicks(-1))).ToList();
            result.Add(new RevenueChartDto
            {
                Label = day.ToString("dd/MM"),
                Date = day,
                Revenue = records.Where(r => r.Amount.HasValue).Sum(r => r.Amount!.Value),
                TicketCount = records.Count
            });
        }
        return result;
    }
}