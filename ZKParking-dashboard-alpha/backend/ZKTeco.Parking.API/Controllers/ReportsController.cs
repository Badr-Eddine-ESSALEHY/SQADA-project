using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZKTeco.Parking.Application.Interfaces;

namespace ZKTeco.Parking.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>Generate daily report as PDF</summary>
    [HttpGet("daily/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyReportPdf(
        [FromQuery] int parkingId = 1,
        [FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var pdfBytes = await _reportService.GenerateDailyReportPdfAsync(parkingId, targetDate);
        return File(pdfBytes, "application/pdf", $"daily-report-{targetDate:yyyy-MM-dd}.pdf");
    }

    /// <summary>Generate daily report as Excel</summary>
    [HttpGet("daily/excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyReportExcel(
        [FromQuery] int parkingId = 1,
        [FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var excelBytes = await _reportService.GenerateDailyReportExcelAsync(parkingId, targetDate);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"daily-report-{targetDate:yyyy-MM-dd}.xlsx");
    }

    /// <summary>Generate monthly report as PDF</summary>
    [HttpGet("monthly/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyReportPdf(
        [FromQuery] int parkingId = 1,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var pdfBytes = await _reportService.GenerateMonthlyReportPdfAsync(parkingId, targetYear, targetMonth);
        return File(pdfBytes, "application/pdf", $"monthly-report-{targetYear}-{targetMonth:D2}.pdf");
    }

    /// <summary>Generate monthly report as Excel</summary>
    [HttpGet("monthly/excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyReportExcel(
        [FromQuery] int parkingId = 1,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var excelBytes = await _reportService.GenerateMonthlyReportExcelAsync(parkingId, targetYear, targetMonth);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"monthly-report-{targetYear}-{targetMonth:D2}.xlsx");
    }
}
