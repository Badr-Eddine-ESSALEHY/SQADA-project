using Microsoft.EntityFrameworkCore;
using ZKTeco.Parking.Domain.Entities;
using ZKTeco.Parking.Domain.Interfaces;
using ZKTeco.Parking.Infrastructure.Data;

namespace ZKTeco.Parking.Infrastructure.Repositories;

public class OperatorRepository : Repository<Operator>, IOperatorRepository
{
    public OperatorRepository(ParkingDbContext context) : base(context) { }

    public async Task<Operator?> GetByUsernameAsync(string username)
    {
        return await _context.Operators
            .FirstOrDefaultAsync(o => o.Username == username);
    }

    public async Task<Operator?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Operators
            .FirstOrDefaultAsync(o => o.RefreshToken == refreshToken);
    }

    public async Task<IEnumerable<Operator>> GetActiveOperatorsAsync()
    {
        return await _context.Operators
            .Where(o => o.IsActive)
            .OrderBy(o => o.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Operator>> GetByParkingIdAsync(int parkingId)
    {
        var parkingIdStr = parkingId.ToString();
        return await _context.Operators
            .Where(o => o.ParkingIds.Contains(parkingIdStr))
            .OrderBy(o => o.FullName)
            .ToListAsync();
    }
}
