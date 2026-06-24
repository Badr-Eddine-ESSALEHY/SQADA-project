using Microsoft.EntityFrameworkCore;
using ZKTeco.Parking.Domain.Entities;
using ZKTeco.Parking.Domain.Interfaces;
using ZKTeco.Parking.Infrastructure.Data;

namespace ZKTeco.Parking.Infrastructure.Repositories;

public class TerminalRepository : Repository<Terminal>, ITerminalRepository
{
    public TerminalRepository(ParkingDbContext context) : base(context) { }

    public async Task<IEnumerable<Terminal>> GetByParkingIdAsync(int parkingId)
    {
        return await _context.Terminals
            .Include(t => t.Parking)
            .Where(t => t.ParkingId == parkingId)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Terminal>> GetActiveTerminalsAsync(int parkingId)
    {
        return await _context.Terminals
            .Include(t => t.Parking)
            .Where(t => t.ParkingId == parkingId && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Terminal>> GetByTypeAsync(int parkingId, string type)
    {
        return await _context.Terminals
            .Include(t => t.Parking)
            .Where(t => t.ParkingId == parkingId && t.Type == type)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(int terminalId, string status, DateTime lastPing)
    {
        var terminal = await _context.Terminals.FindAsync(terminalId);
        if (terminal != null)
        {
            terminal.Status = status;
            terminal.LastPing = lastPing;
            await _context.SaveChangesAsync();
        }
    }
}
