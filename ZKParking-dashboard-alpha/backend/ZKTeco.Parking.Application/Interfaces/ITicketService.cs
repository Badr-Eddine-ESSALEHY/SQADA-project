using ZKTeco.Parking.Application.DTOs;

namespace ZKTeco.Parking.Application.Interfaces;

public interface ITicketService
{
    Task<PagedResultDto<ParkingRecordDto>> GetTicketsAsync(ParkingRecordFilterDto filter);
    Task<ParkingRecordDto?> GetTicketByIdAsync(int id);
    Task<IEnumerable<ParkingRecordDto>> GetActiveTicketsAsync(int parkingId);
}
