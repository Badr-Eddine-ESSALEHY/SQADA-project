using ZKTeco.Parking.Application.DTOs;

namespace ZKTeco.Parking.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(int parkingId, DateTime date);
    Task<IEnumerable<ParkingDto>> GetParkingListAsync();
}
