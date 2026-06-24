using ZKTeco.Parking.Domain.Entities;

namespace ZKTeco.Parking.Domain.Interfaces;

public interface IOperatorRepository : IRepository<Operator>
{
    Task<Operator?> GetByUsernameAsync(string username);
    Task<Operator?> GetByRefreshTokenAsync(string refreshToken);
    Task<IEnumerable<Operator>> GetActiveOperatorsAsync();
    Task<IEnumerable<Operator>> GetByParkingIdAsync(int parkingId);
}
