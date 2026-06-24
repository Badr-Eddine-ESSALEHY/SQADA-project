using Microsoft.Extensions.DependencyInjection;
using ZKTeco.Parking.Application.Interfaces;
using ZKTeco.Parking.Application.Services;

namespace ZKTeco.Parking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ISubscriberService, SubscriberService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
