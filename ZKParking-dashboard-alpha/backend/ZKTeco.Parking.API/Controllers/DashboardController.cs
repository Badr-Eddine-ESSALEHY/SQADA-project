using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZKTeco.Parking.Application.DTOs;
using ZKTeco.Parking.Application.Interfaces;

namespace ZKTeco.Parking.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Get dashboard statistics for a parking on a specific date</summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats([FromQuery] int parkingId = 1, [FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var stats = await _dashboardService.GetDashboardStatsAsync(parkingId, targetDate);
        return Ok(stats);
    }

    /// <summary>Get list of all active parkings</summary>
    [HttpGet("parking-list")]
    [ProducesResponseType(typeof(IEnumerable<ParkingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParkingList()
    {
        var parkings = await _dashboardService.GetParkingListAsync();
        return Ok(parkings);
    }
}
