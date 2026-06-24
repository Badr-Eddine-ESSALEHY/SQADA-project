using ZKTeco.Parking.Application.DTOs;
using ZKTeco.Parking.Application.Interfaces;
using ZKTeco.Parking.Domain.Entities;
using ZKTeco.Parking.Domain.Interfaces;

namespace ZKTeco.Parking.Application.Services;

public class SubscriberService : ISubscriberService
{
    private readonly ISubscriberRepository _repository;

    public SubscriberService(ISubscriberRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SubscriberDto>> GetAllAsync(int parkingId)
    {
        var subscribers = await _repository.GetByParkingIdAsync(parkingId);
        return subscribers.Select(MapToDto);
    }

    public async Task<SubscriberDto?> GetByIdAsync(int id)
    {
        var subscriber = await _repository.GetByIdAsync(id);
        return subscriber == null ? null : MapToDto(subscriber);
    }

    public async Task<SubscriberDto> CreateAsync(CreateSubscriberDto dto)
    {
        var subscriber = new Subscriber
        {
            Name = dto.Name,
            Phone = dto.Phone,
            Email = dto.Email,
            PlateNo = dto.PlateNo,
            CardNo = dto.CardNo,
            SubscriptionType = dto.SubscriptionType,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = "Active",
            Amount = dto.Amount,
            ParkingId = dto.ParkingId,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(subscriber);
        return MapToDto(created);
    }

    public async Task<SubscriberDto?> UpdateAsync(int id, UpdateSubscriberDto dto)
    {
        var subscriber = await _repository.GetByIdAsync(id);
        if (subscriber == null) return null;

        subscriber.Name = dto.Name;
        subscriber.Phone = dto.Phone;
        subscriber.Email = dto.Email;
        subscriber.PlateNo = dto.PlateNo;
        subscriber.CardNo = dto.CardNo;
        subscriber.SubscriptionType = dto.SubscriptionType;
        subscriber.StartDate = dto.StartDate;
        subscriber.EndDate = dto.EndDate;
        subscriber.Status = dto.Status;
        subscriber.Amount = dto.Amount;
        subscriber.Notes = dto.Notes;
        subscriber.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(subscriber);
        return MapToDto(subscriber);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var subscriber = await _repository.GetByIdAsync(id);
        if (subscriber == null) return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task<SubscriberDto?> RenewAsync(int id, RenewSubscriberDto dto)
    {
        var subscriber = await _repository.GetByIdAsync(id);
        if (subscriber == null) return null;

        subscriber.EndDate = dto.NewEndDate;
        subscriber.Amount = dto.Amount;
        subscriber.Status = "Active";
        subscriber.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(subscriber);
        return MapToDto(subscriber);
    }

    public async Task<IEnumerable<SubscriberDto>> GetExpiringAsync(int parkingId, int days)
    {
        var subscribers = await _repository.GetExpiringSubscribersAsync(parkingId, days);
        return subscribers.Select(MapToDto);
    }

    private static SubscriberDto MapToDto(Subscriber s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Phone = s.Phone,
        Email = s.Email,
        PlateNo = s.PlateNo,
        CardNo = s.CardNo,
        SubscriptionType = s.SubscriptionType,
        StartDate = s.StartDate,
        EndDate = s.EndDate,
        Status = s.Status,
        Amount = s.Amount,
        ParkingId = s.ParkingId,
        ParkingName = s.Parking?.Name,
        Notes = s.Notes,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
        DaysUntilExpiry = (int)(s.EndDate - DateTime.UtcNow).TotalDays
    };
}
