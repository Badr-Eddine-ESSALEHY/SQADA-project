using ZKTeco.Parking.Domain.Entities;

namespace ZKTeco.Parking.Domain.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IEnumerable<Payment>> GetByParkingRecordIdAsync(int parkingRecordId);
    Task<IEnumerable<Payment>> GetByDateRangeAsync(int parkingId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalRevenueAsync(int parkingId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Payment>> GetByOperatorIdAsync(int operatorId);
}
