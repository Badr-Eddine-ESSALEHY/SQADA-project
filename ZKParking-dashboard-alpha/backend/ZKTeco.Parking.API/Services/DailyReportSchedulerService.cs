using ZKTeco.Parking.Application.Interfaces;
using ZKTeco.Parking.Domain.Interfaces;

namespace ZKTeco.Parking.API.Services;

public class DailyReportSchedulerService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DailyReportSchedulerService> _logger;
    private readonly IConfiguration _config;

    public DailyReportSchedulerService(
        IServiceProvider services,
        ILogger<DailyReportSchedulerService> logger,
        IConfiguration config)
    {
        _services = services;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Report Scheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextMidnight = now.Date.AddDays(1);
            var delay = nextMidnight - now;

            _logger.LogInformation("Next daily report scheduled at: {time}", nextMidnight);
            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
                await SendDailyReportsAsync(now.Date);
        }
    }

    private async Task SendDailyReportsAsync(DateTime date)
    {
        try
        {
            using var scope = _services.CreateScope();
            var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var parkingRepo = scope.ServiceProvider.GetRequiredService<IParkingRepository>();

            var ownerEmail = _config["EmailSettings:OwnerEmail"] ?? "";
            if (string.IsNullOrEmpty(ownerEmail))
            {
                _logger.LogWarning("OwnerEmail not configured. Skipping daily report.");
                return;
            }

            var parkings = await parkingRepo.GetActiveParkingsAsync();
            foreach (var parking in parkings)
            {
                try
                {
                    _logger.LogInformation("Generating daily report for parking: {name}", parking.Name);
                    var pdf = await reportService.GenerateDailyReportPdfAsync(parking.Id, date);
                    var excel = await reportService.GenerateDailyReportExcelAsync(parking.Id, date);
                    await emailService.SendDailyReportAsync(ownerEmail, parking.Name, date, pdf, excel);
                    _logger.LogInformation("Daily report sent for parking: {name}", parking.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send report for parking: {name}", parking.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Daily report scheduler failed.");
        }
    }
}