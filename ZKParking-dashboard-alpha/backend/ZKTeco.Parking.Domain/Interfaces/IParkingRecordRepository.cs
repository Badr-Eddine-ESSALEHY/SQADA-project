using ZKTeco.Parking.Domain.Entities;

namespace ZKTeco.Parking.Domain.Interfaces;

public interface IParkingRecordRepository : IRepository<ParkingRecord>
{
    Task<IEnumerable<ParkingRecord>> GetByParkingIdAsync(int parkingId);
    Task<IEnumerable<ParkingRecord>> GetByDateRangeAsync(int parkingId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<ParkingRecord>> GetActiveRecordsAsync(int parkingId);
    Task<IEnumerable<ParkingRecord>> GetByCardNoAsync(string cardNo);
    Task<IEnumerable<ParkingRecord>> GetByPlateNoAsync(string plateNo);
    Task<int> GetCurrentOccupancyAsync(int parkingId);
    Task<decimal> GetTotalRevenueAsync(int parkingId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<ParkingRecord>> GetPagedAsync(int parkingId, int page, int pageSize, DateTime? startDate, DateTime? endDate, string? status, string? ticketType);
    Task<int> GetTotalCountAsync(int parkingId, DateTime? startDate, DateTime? endDate, string? status, string? ticketType);
}
