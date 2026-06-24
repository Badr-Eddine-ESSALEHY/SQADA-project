using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZKTeco.Parking.Domain.Interfaces;
using ZKTeco.Parking.Infrastructure.Data;
using ZKTeco.Parking.Infrastructure.Repositories;

namespace ZKTeco.Parking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string not found.");

        services.AddDbContext<ParkingDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register repositories
        services.AddScoped<IParkingRecordRepository, ParkingRecordRepository>();
        services.AddScoped<ISubscriberRepository, SubscriberRepository>();
        services.AddScoped<IOperatorRepository, OperatorRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IGateRepository, GateRepository>();
        services.AddScoped<ITerminalRepository, TerminalRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IParkingRepository, ParkingRepository>();

        return services;
    }
}
