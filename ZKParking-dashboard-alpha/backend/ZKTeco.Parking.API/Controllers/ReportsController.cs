using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZKTeco.Parking.Application.Interfaces;
using ZKTeco.Parking.Domain.Entities;
using ZKTeco.Parking.Infrastructure.Data;

namespace ZKTeco.Parking.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;
    private readonly ParkingDbContext _db;

    public ReportsController(
        IReportService reportService,
        IEmailService emailService,
        IConfiguration config,
        ParkingDbContext db)
    {
        _reportService = reportService;
        _emailService = emailService;
        _config = config;
        _db = db;
    }

    // ── History logging helper ──────────────────────────────────────────────

    private async Task LogGeneratedReportAsync(
        int parkingId, string reportType, string title, string periodLabel,
        string format, long sizeBytes)
    {
        var parking = await _db.Parkings.FindAsync(parkingId);

        _db.GeneratedReports.Add(new GeneratedReport
        {
            ParkingId = parkingId,
            ParkingName = parking?.Name ?? "Parking",
            ReportType = reportType,
            Title = title,
            PeriodLabel = periodLabel,
            Format = format,
            FileSizeBytes = sizeBytes,
            GeneratedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    // ── Recent reports history ──────────────────────────────────────────────

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentReports([FromQuery] int take = 10)
    {
        var items = _db.GeneratedReports
            .OrderByDescending(r => r.GeneratedAt)
            .Take(Math.Clamp(take, 1, 50))
            .Select(r => new
            {
                id = r.Id.ToString(),
                title = r.Title,
                date = r.GeneratedAt,
                format = r.Format,
                size = FormatSize(r.FileSizeBytes)
            })
            .ToList();

        return Ok(items);
    }

    // ── Daily ────────────────────────────────────────────────────────────────

    [HttpPost("daily")]
    public async Task<IActionResult> GetDailyReport([FromBody] ReportRequest request)
    {
        var date = request.Date ?? DateTime.Today;
        var parkingId = request.ParkingId ?? 1;
        var periodLabel = date.ToString("dd/MM/yyyy");

        if (request.Format == "excel")
        {
            var excel = await _reportService.GenerateDailyReportExcelAsync(parkingId, date);
            await LogGeneratedReportAsync(parkingId, "daily", "Rapport Journalier", periodLabel, "EXCEL", excel.Length);
            return File(excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"rapport-journalier-{date:yyyy-MM-dd}.xlsx");
        }

        var pdf = await _reportService.GenerateDailyReportPdfAsync(parkingId, date);
        await LogGeneratedReportAsync(parkingId, "daily", "Rapport Journalier", periodLabel, "PDF", pdf.Length);
        return File(pdf, "application/pdf", $"rapport-journalier-{date:yyyy-MM-dd}.pdf");
    }

    // ── Weekly ───────────────────────────────────────────────────────────────

    [HttpPost("weekly")]
    public async Task<IActionResult> GetWeeklyReport([FromBody] ReportRequest request)
    {
        var date = request.Date ?? DateTime.Today;
        var weekStart = date.AddDays(-(int)date.DayOfWeek + 1);
        var weekEnd = weekStart.AddDays(6);
        var parkingId = request.ParkingId ?? 1;
        var periodLabel = $"{weekStart:dd/MM} - {weekEnd:dd/MM/yyyy}";

        if (request.Format == "excel")
        {
            var excel = await _reportService.GenerateWeeklyReportExcelAsync(parkingId, weekStart);
            await LogGeneratedReportAsync(parkingId, "weekly", "Rapport Hebdomadaire", periodLabel, "EXCEL", excel.Length);
            return File(excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"rapport-hebdomadaire-{weekStart:yyyy-MM-dd}.xlsx");
        }

        var pdf = await _reportService.GenerateWeeklyReportPdfAsync(parkingId, weekStart);
        await LogGeneratedReportAsync(parkingId, "weekly", "Rapport Hebdomadaire", periodLabel, "PDF", pdf.Length);
        return File(pdf, "application/pdf", $"rapport-hebdomadaire-{weekStart:yyyy-MM-dd}.pdf");
    }

    // ── Monthly ──────────────────────────────────────────────────────────────
    // NOTE: the frontend's <input type="month"> sends "yyyy-MM" (e.g. "2026-06"),
    // which the default model binder cannot parse into DateTime. The frontend has
    // been fixed to normalize this to "yyyy-MM-01" before sending, so this stays
    // a plain ReportRequest with no DTO change required.

    [HttpPost("monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromBody] ReportRequest request)
    {
        var date = request.Date ?? DateTime.Today;
        var parkingId = request.ParkingId ?? 1;
        var periodLabel = date.ToString("MMMM yyyy");

        if (request.Format == "excel")
        {
            var excel = await _reportService.GenerateMonthlyReportExcelAsync(parkingId, date.Year, date.Month);
            await LogGeneratedReportAsync(parkingId, "monthly", "Rapport Mensuel", periodLabel, "EXCEL", excel.Length);
            return File(excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"rapport-mensuel-{date:yyyy-MM}.xlsx");
        }

        var pdf = await _reportService.GenerateMonthlyReportPdfAsync(parkingId, date.Year, date.Month);
        await LogGeneratedReportAsync(parkingId, "monthly", "Rapport Mensuel", periodLabel, "PDF", pdf.Length);
        return File(pdf, "application/pdf", $"rapport-mensuel-{date:yyyy-MM}.pdf");
    }

    // ── Annual ───────────────────────────────────────────────────────────────

    [HttpPost("annual")]
    public async Task<IActionResult> GetAnnualReport([FromBody] ReportRequest request)
    {
        var date = request.Date ?? DateTime.Today;
        var parkingId = request.ParkingId ?? 1;
        var periodLabel = date.Year.ToString();

        if (request.Format == "excel")
        {
            var excel = await _reportService.GenerateAnnualReportExcelAsync(parkingId, date.Year);
            await LogGeneratedReportAsync(parkingId, "annual", "Rapport Annuel", periodLabel, "EXCEL", excel.Length);
            return File(excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"rapport-annuel-{date.Year}.xlsx");
        }

        var pdf = await _reportService.GenerateAnnualReportPdfAsync(parkingId, date.Year);
        await LogGeneratedReportAsync(parkingId, "annual", "Rapport Annuel", periodLabel, "PDF", pdf.Length);
        return File(pdf, "application/pdf", $"rapport-annuel-{date.Year}.pdf");
    }

    // ── Custom date range ────────────────────────────────────────────────────

    [HttpPost("custom")]
    public async Task<IActionResult> GetCustomReport([FromBody] CustomReportRequest request)
    {
        if (request.StartDate is null || request.EndDate is null)
            return BadRequest(new { message = "startDate et endDate sont requis." });

        var start = request.StartDate.Value.Date;
        var end = request.EndDate.Value.Date;

        if (end < start)
            return BadRequest(new { message = "endDate doit être postérieure ou égale à startDate." });

        var parkingId = request.ParkingId ?? 1;
        var periodLabel = $"{start:dd/MM/yyyy} - {end:dd/MM/yyyy}";

        if (request.Format == "excel")
        {
            var excel = await _reportService.GenerateCustomReportExcelAsync(parkingId, start, end);
            await LogGeneratedReportAsync(parkingId, "custom", "Rapport Personnalisé", periodLabel, "EXCEL", excel.Length);
            return File(excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"rapport-personnalise-{start:yyyy-MM-dd}_{end:yyyy-MM-dd}.xlsx");
        }

        var pdf = await _reportService.GenerateCustomReportPdfAsync(parkingId, start, end);
        await LogGeneratedReportAsync(parkingId, "custom", "Rapport Personnalisé", periodLabel, "PDF", pdf.Length);
        return File(pdf, "application/pdf", $"rapport-personnalise-{start:yyyy-MM-dd}_{end:yyyy-MM-dd}.pdf");
    }

    // ── Test email ───────────────────────────────────────────────────────────

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

public class CustomReportRequest
{
    public int? ParkingId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Format { get; set; } = "pdf";
}