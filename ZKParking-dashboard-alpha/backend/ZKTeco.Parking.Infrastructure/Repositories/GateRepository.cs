using Microsoft.EntityFrameworkCore;
using ZKTeco.Parking.Domain.Entities;
using ZKTeco.Parking.Domain.Interfaces;
using ZKTeco.Parking.Infrastructure.Data;

namespace ZKTeco.Parking.Infrastructure.Repositories;

public class GateRepository : Repository<Gate>, IGateRepository
{
    public GateRepository(ParkingDbContext context) : base(context) { }

    public async Task<IEnumerable<Gate>> GetByParkingIdAsync(int parkingId)
    {
        return await _context.Gates
            .Include(g => g.Parking)
            .Where(g => g.ParkingId == parkingId)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Gate>> GetActiveGatesAsync(int parkingId)
    {
        return await _context.Gates
            .Include(g => g.Parking)
            .Where(g => g.ParkingId == parkingId && g.IsActive)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Gate>> GetByTypeAsync(int parkingId, string type)
    {
        return await _context.Gates
            .Include(g => g.Parking)
            .Where(g => g.ParkingId == parkingId && g.Type == type)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(int gateId, string status, DateTime lastPing)
    {
        var gate = await _context.Gates.FindAsync(gateId);
        if (gate != null)
        {
            gate.Status = status;
            gate.LastPing = lastPing;
            await _context.SaveChangesAsync();
        }
    }
}
