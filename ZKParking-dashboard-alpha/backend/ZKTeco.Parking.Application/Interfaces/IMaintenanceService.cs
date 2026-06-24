using ZKTeco.Parking.Application.DTOs;

namespace ZKTeco.Parking.Application.Interfaces;

public interface IMaintenanceService
{
    Task<IEnumerable<GateDto>> GetGatesAsync(int parkingId);
    Task<IEnumerable<TerminalDto>> GetTerminalsAsync(int parkingId);
    Task<IEnumerable<AlertDto>> GetAlertsAsync(int parkingId);
    Task<IEnumerable<AlertDto>> GetUnreadAlertsAsync(int parkingId);
    Task<bool> MarkAlertAsReadAsync(int alertId);
    Task<bool> MarkAllAlertsAsReadAsync(int parkingId);
}
