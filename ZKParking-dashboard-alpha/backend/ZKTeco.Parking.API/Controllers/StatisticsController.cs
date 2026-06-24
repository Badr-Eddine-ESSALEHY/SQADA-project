using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZKTeco.Parking.Application.DTOs;
using ZKTeco.Parking.Application.Interfaces;

namespace ZKTeco.Parking.API.Controllers;

[ApiController]
[Route("api/statistics")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>Get daily revenue chart data for a date range</summary>
    [HttpGet("daily-revenue")]
    [ProducesResponseType(typeof(IEnumerable<RevenueChartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyRevenue(
        [FromQuery] int parkingId = 1,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        var data = await _statisticsService.GetDailyRevenueAsync(parkingId, start, end);
        return Ok(data);
    }

    /// <summary>Get monthly revenue chart data for a year</summary>
    [HttpGet("monthly-revenue")]
    [ProducesResponseType(typeof(IEnumerable<MonthlyRevenueDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyRevenue(
        [FromQuery] int parkingId = 1,
        [FromQuery] int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var data = await _statisticsService.GetMonthlyRevenueAsync(parkingId, targetYear);
        return Ok(data);
    }

    /// <summary>Get occupancy by hour for a specific date</summary>
    [HttpGet("occupancy-by-hour")]
    [ProducesResponseType(typeof(IEnumerable<OccupancyChartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOccupancyByHour(
        [FromQuery] int parkingId = 1,
        [FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var data = await _statisticsService.GetOccupancyByHourAsync(parkingId, targetDate);
        return Ok(data);
    }

    /// <summary>Get ticket type distribution for a specific date</summary>
    [HttpGet("ticket-types")]
    [ProducesResponseType(typeof(IEnumerable<TicketTypeDistributionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTicketTypeDistribution(
        [FromQuery] int parkingId = 1,
        [FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var data = await _statisticsService.GetTicketTypeDistributionAsync(parkingId, targetDate);
        return Ok(data);
    }

    /// <summary>Get average parking duration by day for the last N days</summary>
    [HttpGet("average-duration")]
    [ProducesResponseType(typeof(IEnumerable<AverageDurationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAverageDuration(
        [FromQuery] int parkingId = 1,
        [FromQuery] int days = 30)
    {
        var data = await _statisticsService.GetAverageDurationByDayAsync(parkingId, days);
        return Ok(data);
    }
}
