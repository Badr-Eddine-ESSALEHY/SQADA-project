using Microsoft.EntityFrameworkCore;
using ZKTeco.Parking.Domain.Interfaces;
using ZKTeco.Parking.Infrastructure.Data;

namespace ZKTeco.Parking.Infrastructure.Repositories;

public class ParkingRepository : Repository<Domain.Entities.Parking>, IParkingRepository
{
    public ParkingRepository(ParkingDbContext context) : base(context) { }

    public async Task<IEnumerable<Domain.Entities.Parking>> GetActiveParkingsAsync()
    {
        return await _context.Parkings
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Domain.Entities.Parking?> GetByCodeAsync(string code)
    {
        return await _context.Parkings
            .FirstOrDefaultAsync(p => p.Code == code);
    }
}
