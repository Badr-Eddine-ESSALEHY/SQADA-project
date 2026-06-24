using ZKTeco.Parking.Application.DTOs;

namespace ZKTeco.Parking.Application.Interfaces;

public interface ISubscriberService
{
    Task<IEnumerable<SubscriberDto>> GetAllAsync(int parkingId);
    Task<SubscriberDto?> GetByIdAsync(int id);
    Task<SubscriberDto> CreateAsync(CreateSubscriberDto dto);
    Task<SubscriberDto?> UpdateAsync(int id, UpdateSubscriberDto dto);
    Task<bool> DeleteAsync(int id);
    Task<SubscriberDto?> RenewAsync(int id, RenewSubscriberDto dto);
    Task<IEnumerable<SubscriberDto>> GetExpiringAsync(int parkingId, int days);
}
