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
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public ReportsController(IReportService reportService, IEmailService emailService, IConfiguration config)
    {
        _reportService = reportService;
        _emailService = emailService;
        _config = config;
    }

    [HttpPost("daily")]
    public async Task<IActionResult> GetDailyReport(
        [FromBody] ReportRequest request)
    {
        var date = request.Date ?? DateTime.Today;
        var parkingId = request.ParkingId ?? 1;

        if (request.Format == "excel")
        {
            var excel = await _reportService.GenerateDailyReportExcelAsync(parkingId, date);
            return File(excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"rapport-journalier-{date:yyyy-MM-dd}.xlsx");
        }
        var pdf = await _reportService.GenerateDailyReportPdfAsync(parkingId, date);
        return File(pdf, "application/pdf", $"rapport-journalier-{date:yyyy-MM-dd}.pdf");
    }

    [HttpPost("weekly")]
    public async Task<IActionResult> GetWeeklyReport([FromBody] ReportRequest request)
    {
        var date = request.Date ?? DateTime.Today;
        var weekStart = date.AddDays(-(int)date.DayOfWeek + 1);
        var parkingId = request.ParkingId ?? 1;

        if (request.Format == "excel")
        {
            var excel = await _reportService.GenerateWeeklyReportExcelAsync(parkingId, weekStart);
            return File(excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"rapport-hebdomadaire-{weekStart:yyyy-MM-dd}.xlsx");
        }
        var pdf = await _reportService.GenerateWeeklyReportPdfAsync(parkingId, weekStart);
        return File(pdf, "application/pdf", $"rapport-hebdomadaire-{weekStart:yyyy-MM-dd}.pdf");
    }

    [HttpPost("monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromBody] ReportRequest request)
    {
        var date = request.Date ?? DateTime.Today;
        var parkingId = request.ParkingId ?? 1;

        if (request.Format == "excel")
        {
            var excel = await _reportService.GenerateMonthlyReportExcelAsync(parkingId, date.Year, date.Month);
            return File(excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"rapport-mensuel-{date:yyyy-MM}.xlsx");
        }
        var pdf = await _reportService.GenerateMonthlyReportPdfAsync(parkingId, date.Year, date.Month);
        return File(pdf, "application/pdf", $"rapport-mensuel-{date:yyyy-MM}.pdf");
    }

    [HttpPost("annual")]
    public async Task<IActionResult> GetAnnualReport([FromBody] ReportRequest request)
    {
        var date = request.Date ?? DateTime.Today;
        var parkingId = request.ParkingId ?? 1;

        if (request.Format == "excel")
        {
            var excel = await _reportService.GenerateAnnualReportExcelAsync(parkingId, date.Year);
            return File(excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"rapport-annuel-{date.Year}.xlsx");
        }
        var pdf = await _reportService.GenerateAnnualReportPdfAsync(parkingId, date.Year);
        return File(pdf, "application/pdf", $"rapport-annuel-{date.Year}.pdf");
    }

    [HttpPost("send-test-email")]
    public async Task<IActionResult> SendTestEmail()
    {
        var ownerEmail = _config["EmailSettings:OwnerEmail"] ?? "";
        if (string.IsNullOrEmpty(ownerEmail))
            return BadRequest(new { message = "OwnerEmail not configured in appsettings.json" });

        var pdf = await _reportService.GenerateDailyReportPdfAsync(1, DateTime.Today);
        var excel = await _reportService.GenerateDailyReportExcelAsync(1, DateTime.Today);
        await _emailService.SendDailyReportAsync(ownerEmail, "Parking Central", DateTime.Today, pdf, excel);
        return Ok(new { message = "Test email sent successfully" });
    }
}

public class ReportRequest
{
    public int? ParkingId { get; set; }
    public DateTime? Date { get; set; }
    public string Format { get; set; } = "pdf";
}