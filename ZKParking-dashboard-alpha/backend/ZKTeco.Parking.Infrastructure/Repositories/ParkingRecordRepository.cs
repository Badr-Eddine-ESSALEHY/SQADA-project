using Microsoft.EntityFrameworkCore;
using ZKTeco.Parking.Domain.Entities;
using ZKTeco.Parking.Domain.Interfaces;
using ZKTeco.Parking.Infrastructure.Data;

namespace ZKTeco.Parking.Infrastructure.Repositories;

public class ParkingRecordRepository : Repository<ParkingRecord>, IParkingRecordRepository
{
    public ParkingRecordRepository(ParkingDbContext context) : base(context) { }

    public override async Task<ParkingRecord?> GetByIdAsync(int id)
    {
        return await _context.ParkingRecords
            .Include(r => r.Parking)
            .Include(r => r.Operator)
            .Include(r => r.GateIn)
            .Include(r => r.GateOut)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<ParkingRecord>> GetByParkingIdAsync(int parkingId)
    {
        return await _context.ParkingRecords
            .Include(r => r.Parking)
            .Include(r => r.Operator)
            .Include(r => r.GateIn)
            .Include(r => r.GateOut)
            .Where(r => r.ParkingId == parkingId)
            .OrderByDescending(r => r.EntryTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParkingRecord>> GetByDateRangeAsync(int parkingId, DateTime startDate, DateTime endDate)
    {
        return await _context.ParkingRecords
            .Include(r => r.Parking)
            .Include(r => r.Operator)
            .Include(r => r.GateIn)
            .Include(r => r.GateOut)
            .Where(r => r.ParkingId == parkingId && r.EntryTime >= startDate && r.EntryTime <= endDate)
            .OrderByDescending(r => r.EntryTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParkingRecord>> GetActiveRecordsAsync(int parkingId)
    {
        return await _context.ParkingRecords
            .Include(r => r.Parking)
            .Include(r => r.Operator)
            .Include(r => r.GateIn)
            .Where(r => r.ParkingId == parkingId && r.ExitTime == null)
            .OrderByDescending(r => r.EntryTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParkingRecord>> GetByCardNoAsync(string cardNo)
    {
        return await _context.ParkingRecords
            .Where(r => r.CardNo == cardNo)
            .OrderByDescending(r => r.EntryTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParkingRecord>> GetByPlateNoAsync(string plateNo)
    {
        return await _context.ParkingRecords
            .Where(r => r.PlateNo == plateNo)
            .OrderByDescending(r => r.EntryTime)
            .ToListAsync();
    }

    public async Task<int> GetCurrentOccupancyAsync(int parkingId)
    {
        return await _context.ParkingRecords
            .CountAsync(r => r.ParkingId == parkingId && r.ExitTime == null);
    }

    public async Task<decimal> GetTotalRevenueAsync(int parkingId, DateTime startDate, DateTime endDate)
    {
        return await _context.ParkingRecords
            .Where(r => r.ParkingId == parkingId && r.EntryTime >= startDate && r.EntryTime <= endDate && r.Amount.HasValue)
            .SumAsync(r => r.Amount!.Value);
    }

    public async Task<IEnumerable<ParkingRecord>> GetPagedAsync(int parkingId, int page, int pageSize, DateTime? startDate, DateTime? endDate, string? status, string? ticketType)
    {
        var query = _context.ParkingRecords
            .Include(r => r.Parking)
            .Include(r => r.Operator)
            .Include(r => r.GateIn)
            .Include(r => r.GateOut)
            .Where(r => r.ParkingId == parkingId);

        if (startDate.HasValue) query = query.Where(r => r.EntryTime >= startDate.Value);
        if (endDate.HasValue) query = query.Where(r => r.EntryTime <= endDate.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);
        if (!string.IsNullOrEmpty(ticketType)) query = query.Where(r => r.TicketType == ticketType);

        return await query
            .OrderByDescending(r => r.EntryTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(int parkingId, DateTime? startDate, DateTime? endDate, string? status, string? ticketType)
    {
        var query = _context.ParkingRecords.Where(r => r.ParkingId == parkingId);

        if (startDate.HasValue) query = query.Where(r => r.EntryTime >= startDate.Value);
        if (endDate.HasValue) query = query.Where(r => r.EntryTime <= endDate.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);
        if (!string.IsNullOrEmpty(ticketType)) query = query.Where(r => r.TicketType == ticketType);

        return await query.CountAsync();
    }
}
