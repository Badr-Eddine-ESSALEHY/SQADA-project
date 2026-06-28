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

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(
        [FromQuery] int parkingId = 1,
        [FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var stats = await _dashboardService.GetDashboardStatsAsync(parkingId, targetDate);
        return Ok(stats);
    }

    [HttpGet("parking-list")]
    public async Task<IActionResult> GetParkingList()
    {
        var parkings = await _dashboardService.GetParkingListAsync();
        return Ok(parkings);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] int parkingId = 1,
        [FromQuery] int days = 30)
    {
        var result = await _dashboardService.GetRevenueChartAsync(parkingId, days);
        return Ok(result);
    }
}