using Microsoft.EntityFrameworkCore;
using ZKTeco.Parking.Domain.Entities;
using ZKTeco.Parking.Domain.Interfaces;
using ZKTeco.Parking.Infrastructure.Data;

namespace ZKTeco.Parking.Infrastructure.Repositories;

public class AlertRepository : Repository<Alert>, IAlertRepository
{
    public AlertRepository(ParkingDbContext context) : base(context) { }

    public async Task<IEnumerable<Alert>> GetByParkingIdAsync(int parkingId)
    {
        return await _context.Alerts
            .Include(a => a.Parking)
            .Where(a => a.ParkingId == parkingId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alert>> GetUnreadAlertsAsync(int parkingId)
    {
        return await _context.Alerts
            .Include(a => a.Parking)
            .Where(a => a.ParkingId == parkingId && !a.IsRead)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int alertId)
    {
        var alert = await _context.Alerts.FindAsync(alertId);
        if (alert != null)
        {
            alert.IsRead = true;
            alert.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(int parkingId)
    {
        var alerts = await _context.Alerts
            .Where(a => a.ParkingId == parkingId && !a.IsRead)
            .ToListAsync();

        foreach (var alert in alerts)
        {
            alert.IsRead = true;
            alert.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(int parkingId)
    {
        return await _context.Alerts
            .CountAsync(a => a.ParkingId == parkingId && !a.IsRead);
    }
}
