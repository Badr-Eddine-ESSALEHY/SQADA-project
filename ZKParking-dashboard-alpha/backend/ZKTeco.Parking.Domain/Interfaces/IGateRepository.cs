using ZKTeco.Parking.Domain.Entities;

namespace ZKTeco.Parking.Domain.Interfaces;

public interface IGateRepository : IRepository<Gate>
{
    Task<IEnumerable<Gate>> GetByParkingIdAsync(int parkingId);
    Task<IEnumerable<Gate>> GetActiveGatesAsync(int parkingId);
    Task<IEnumerable<Gate>> GetByTypeAsync(int parkingId, string type);
    Task UpdateStatusAsync(int gateId, string status, DateTime lastPing);
}
