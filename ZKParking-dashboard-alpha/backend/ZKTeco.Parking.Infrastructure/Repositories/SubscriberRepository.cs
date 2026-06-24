using Microsoft.EntityFrameworkCore;
using ZKTeco.Parking.Domain.Entities;
using ZKTeco.Parking.Domain.Interfaces;
using ZKTeco.Parking.Infrastructure.Data;

namespace ZKTeco.Parking.Infrastructure.Repositories;

public class SubscriberRepository : Repository<Subscriber>, ISubscriberRepository
{
    public SubscriberRepository(ParkingDbContext context) : base(context) { }

    public override async Task<Subscriber?> GetByIdAsync(int id)
    {
        return await _context.Subscribers
            .Include(s => s.Parking)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Subscriber>> GetByParkingIdAsync(int parkingId)
    {
        return await _context.Subscribers
            .Include(s => s.Parking)
            .Where(s => s.ParkingId == parkingId)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscriber>> GetActiveSubscribersAsync(int parkingId)
    {
        return await _context.Subscribers
            .Include(s => s.Parking)
            .Where(s => s.ParkingId == parkingId && s.Status == "Active" && s.EndDate >= DateTime.UtcNow)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subscriber>> GetExpiringSubscribersAsync(int parkingId, int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(days);
        return await _context.Subscribers
            .Include(s => s.Parking)
            .Where(s => s.ParkingId == parkingId && s.Status == "Active" && s.EndDate <= cutoffDate && s.EndDate >= DateTime.UtcNow)
            .OrderBy(s => s.EndDate)
            .ToListAsync();
    }

    public async Task<Subscriber?> GetByCardNoAsync(string cardNo)
    {
        return await _context.Subscribers
            .Include(s => s.Parking)
            .FirstOrDefaultAsync(s => s.CardNo == cardNo);
    }

    public async Task<Subscriber?> GetByPlateNoAsync(string plateNo)
    {
        return await _context.Subscribers
            .Include(s => s.Parking)
            .FirstOrDefaultAsync(s => s.PlateNo == plateNo);
    }

    public async Task<int> GetActiveCountAsync(int parkingId)
    {
        return await _context.Subscribers
            .CountAsync(s => s.ParkingId == parkingId && s.Status == "Active" && s.EndDate >= DateTime.UtcNow);
    }
}
