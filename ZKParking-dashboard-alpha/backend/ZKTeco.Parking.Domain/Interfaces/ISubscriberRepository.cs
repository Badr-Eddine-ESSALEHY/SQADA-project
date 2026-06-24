using ZKTeco.Parking.Domain.Entities;

namespace ZKTeco.Parking.Domain.Interfaces;

public interface ISubscriberRepository : IRepository<Subscriber>
{
    Task<IEnumerable<Subscriber>> GetByParkingIdAsync(int parkingId);
    Task<IEnumerable<Subscriber>> GetActiveSubscribersAsync(int parkingId);
    Task<IEnumerable<Subscriber>> GetExpiringSubscribersAsync(int parkingId, int days);
    Task<Subscriber?> GetByCardNoAsync(string cardNo);
    Task<Subscriber?> GetByPlateNoAsync(string plateNo);
    Task<int> GetActiveCountAsync(int parkingId);
}
