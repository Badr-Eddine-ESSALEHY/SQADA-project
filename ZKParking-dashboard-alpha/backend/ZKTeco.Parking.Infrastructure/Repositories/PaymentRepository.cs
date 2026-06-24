using Microsoft.EntityFrameworkCore;
using ZKTeco.Parking.Domain.Entities;
using ZKTeco.Parking.Domain.Interfaces;
using ZKTeco.Parking.Infrastructure.Data;

namespace ZKTeco.Parking.Infrastructure.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ParkingDbContext context) : base(context) { }

    public async Task<IEnumerable<Payment>> GetByParkingRecordIdAsync(int parkingRecordId)
    {
        return await _context.Payments
            .Include(p => p.ParkingRecord)
            .Include(p => p.Operator)
            .Where(p => p.ParkingRecordId == parkingRecordId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByDateRangeAsync(int parkingId, DateTime startDate, DateTime endDate)
    {
        return await _context.Payments
            .Include(p => p.ParkingRecord)
            .Include(p => p.Operator)
            .Where(p => p.ParkingRecord!.ParkingId == parkingId && p.PaidAt >= startDate && p.PaidAt <= endDate)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync(int parkingId, DateTime startDate, DateTime endDate)
    {
        return await _context.Payments
            .Where(p => p.ParkingRecord!.ParkingId == parkingId && p.PaidAt >= startDate && p.PaidAt <= endDate && p.Status == "Completed")
            .SumAsync(p => p.Amount);
    }

    public async Task<IEnumerable<Payment>> GetByOperatorIdAsync(int operatorId)
    {
        return await _context.Payments
            .Include(p => p.ParkingRecord)
            .Where(p => p.OperatorId == operatorId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();
    }
}
