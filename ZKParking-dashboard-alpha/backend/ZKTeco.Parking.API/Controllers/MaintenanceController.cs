using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZKTeco.Parking.Application.DTOs;
using ZKTeco.Parking.Application.Interfaces;

namespace ZKTeco.Parking.API.Controllers;

[ApiController]
[Route("api/maintenance")]
[Authorize]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;

    public MaintenanceController(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    /// <summary>Get all gates for a parking</summary>
    [HttpGet("gates")]
    [ProducesResponseType(typeof(IEnumerable<GateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGates([FromQuery] int parkingId = 1)
    {
        var gates = await _maintenanceService.GetGatesAsync(parkingId);
        return Ok(gates);
    }

    /// <summary>Get all terminals for a parking</summary>
    [HttpGet("terminals")]
    [ProducesResponseType(typeof(IEnumerable<TerminalDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTerminals([FromQuery] int parkingId = 1)
    {
        var terminals = await _maintenanceService.GetTerminalsAsync(parkingId);
        return Ok(terminals);
    }

    /// <summary>Get all alerts for a parking</summary>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(IEnumerable<AlertDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlerts([FromQuery] int parkingId = 1, [FromQuery] bool unreadOnly = false)
    {
        IEnumerable<AlertDto> alerts;
        if (unreadOnly)
            alerts = await _maintenanceService.GetUnreadAlertsAsync(parkingId);
        else
            alerts = await _maintenanceService.GetAlertsAsync(parkingId);
        return Ok(alerts);
    }

    /// <summary>Mark a specific alert as read</summary>
    [HttpPut("alerts/{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAlertAsRead(int id)
    {
        var result = await _maintenanceService.MarkAlertAsReadAsync(id);
        if (!result) return NotFound(new { message = $"Alert with ID {id} not found" });
        return Ok(new { message = "Alert marked as read" });
    }

    /// <summary>Mark all alerts as read for a parking</summary>
    [HttpPut("alerts/read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAlertsAsRead([FromQuery] int parkingId = 1)
    {
        await _maintenanceService.MarkAllAlertsAsReadAsync(parkingId);
        return Ok(new { message = "All alerts marked as read" });
    }
}
