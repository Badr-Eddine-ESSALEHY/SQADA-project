using ZKTeco.Parking.Application.DTOs;
using ZKTeco.Parking.Application.Interfaces;
using ZKTeco.Parking.Domain.Interfaces;

namespace ZKTeco.Parking.Application.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly IGateRepository _gateRepository;
    private readonly ITerminalRepository _terminalRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IParkingRepository _parkingRepository;

    public MaintenanceService(
        IGateRepository gateRepository,
        ITerminalRepository terminalRepository,
        IAlertRepository alertRepository,
        IParkingRepository parkingRepository)
    {
        _gateRepository = gateRepository;
        _terminalRepository = terminalRepository;
        _alertRepository = alertRepository;
        _parkingRepository = parkingRepository;
    }

    public async Task<IEnumerable<GateDto>> GetGatesAsync(int parkingId)
    {
        var gates = await _gateRepository.GetByParkingIdAsync(parkingId);
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        return gates.Select(g => new GateDto
        {
            Id = g.Id,
            ParkingId = g.ParkingId,
            ParkingName = parking?.Name ?? string.Empty,
            Name = g.Name,
            Type = g.Type,
            IpAddress = g.IpAddress,
            Status = g.Status,
            LastPing = g.LastPing,
            IsActive = g.IsActive
        });
    }

    public async Task<IEnumerable<TerminalDto>> GetTerminalsAsync(int parkingId)
    {
        var terminals = await _terminalRepository.GetByParkingIdAsync(parkingId);
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        return terminals.Select(t => new TerminalDto
        {
            Id = t.Id,
            ParkingId = t.ParkingId,
            ParkingName = parking?.Name ?? string.Empty,
            Name = t.Name,
            Type = t.Type,
            IpAddress = t.IpAddress,
            Status = t.Status,
            LastPing = t.LastPing,
            FirmwareVersion = t.FirmwareVersion,
            IsActive = t.IsActive
        });
    }

    public async Task<IEnumerable<AlertDto>> GetAlertsAsync(int parkingId)
    {
        var alerts = await _alertRepository.GetByParkingIdAsync(parkingId);
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        return alerts.Select(a => MapAlertToDto(a, parking?.Name ?? string.Empty));
    }

    public async Task<IEnumerable<AlertDto>> GetUnreadAlertsAsync(int parkingId)
    {
        var alerts = await _alertRepository.GetUnreadAlertsAsync(parkingId);
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        return alerts.Select(a => MapAlertToDto(a, parking?.Name ?? string.Empty));
    }

    public async Task<bool> MarkAlertAsReadAsync(int alertId)
    {
        var alert = await _alertRepository.GetByIdAsync(alertId);
        if (alert == null) return false;
        await _alertRepository.MarkAsReadAsync(alertId);
        return true;
    }

    public async Task<bool> MarkAllAlertsAsReadAsync(int parkingId)
    {
        await _alertRepository.MarkAllAsReadAsync(parkingId);
        return true;
    }

    private static AlertDto MapAlertToDto(Domain.Entities.Alert a, string parkingName) => new()
    {
        Id = a.Id,
        ParkingId = a.ParkingId,
        ParkingName = parkingName,
        Type = a.Type,
        Message = a.Message,
        Severity = a.Severity,
        IsRead = a.IsRead,
        CreatedAt = a.CreatedAt,
        ReadAt = a.ReadAt
    };
}
