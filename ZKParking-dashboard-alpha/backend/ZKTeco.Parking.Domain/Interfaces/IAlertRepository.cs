using ZKTeco.Parking.Domain.Entities;

namespace ZKTeco.Parking.Domain.Interfaces;

public interface IAlertRepository : IRepository<Alert>
{
    Task<IEnumerable<Alert>> GetByParkingIdAsync(int parkingId);
    Task<IEnumerable<Alert>> GetUnreadAlertsAsync(int parkingId);
    Task MarkAsReadAsync(int alertId);
    Task MarkAllAsReadAsync(int parkingId);
    Task<int> GetUnreadCountAsync(int parkingId);
}
