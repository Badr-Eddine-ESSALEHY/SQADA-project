using ZKTeco.Parking.Application.DTOs;
using ZKTeco.Parking.Application.Interfaces;
using ZKTeco.Parking.Domain.Interfaces;

namespace ZKTeco.Parking.Application.Services;

public class TicketService : ITicketService
{
    private readonly IParkingRecordRepository _repository;

    public TicketService(IParkingRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<ParkingRecordDto>> GetTicketsAsync(ParkingRecordFilterDto filter)
    {
        var records = await _repository.GetPagedAsync(
            filter.ParkingId,
            filter.Page,
            filter.PageSize,
            filter.StartDate,
            filter.EndDate,
            filter.Status,
            filter.TicketType);

        var totalCount = await _repository.GetTotalCountAsync(
            filter.ParkingId,
            filter.StartDate,
            filter.EndDate,
            filter.Status,
            filter.TicketType);

        var items = records.Select(MapToDto);

        return new PagedResultDto<ParkingRecordDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<ParkingRecordDto?> GetTicketByIdAsync(int id)
    {
        var record = await _repository.GetByIdAsync(id);
        return record == null ? null : MapToDto(record);
    }

    public async Task<IEnumerable<ParkingRecordDto>> GetActiveTicketsAsync(int parkingId)
    {
        var records = await _repository.GetActiveRecordsAsync(parkingId);
        return records.Select(MapToDto);
    }

    private static ParkingRecordDto MapToDto(Domain.Entities.ParkingRecord r) => new()
    {
        Id = r.Id,
        CardNo = r.CardNo,
        PlateNo = r.PlateNo,
        EntryTime = r.EntryTime,
        ExitTime = r.ExitTime,
        DurationMinutes = r.Duration?.TotalMinutes,
        Amount = r.Amount,
        TicketType = r.TicketType,
        Status = r.Status,
        ParkingId = r.ParkingId,
        ParkingName = r.Parking?.Name,
        OperatorId = r.OperatorId,
        OperatorName = r.Operator?.FullName,
        GateInId = r.GateInId,
        GateInName = r.GateIn?.Name,
        GateOutId = r.GateOutId,
        GateOutName = r.GateOut?.Name,
        CreatedAt = r.CreatedAt
    };
}
