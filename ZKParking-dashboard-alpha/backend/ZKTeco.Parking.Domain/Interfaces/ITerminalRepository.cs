using ZKTeco.Parking.Domain.Entities;

namespace ZKTeco.Parking.Domain.Interfaces;

public interface ITerminalRepository : IRepository<Terminal>
{
    Task<IEnumerable<Terminal>> GetByParkingIdAsync(int parkingId);
    Task<IEnumerable<Terminal>> GetActiveTerminalsAsync(int parkingId);
    Task<IEnumerable<Terminal>> GetByTypeAsync(int parkingId, string type);
    Task UpdateStatusAsync(int terminalId, string status, DateTime lastPing);
}
