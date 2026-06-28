using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ZKTeco.Parking.Infrastructure.Data;
using ZKTeco.Parking.Domain.Interfaces;
using ZKTeco.Parking.Infrastructure.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Liaison de la base PostgreSQL
        services.AddDbContext<ParkingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // 2. Enregistrement des Repositories pour le groupe 1 (Gestion des Enregistrements et des Portes)
        services.AddScoped<IParkingRecordRepository, ParkingRecordRepository>();
        services.AddScoped<IGateRepository, GateRepository>();

        // 3. Enregistrement des Repositories pour le groupe 2 (Gestion du Parking global et des Terminaux)
        services.AddScoped<IParkingRepository, ParkingRepository>();
        services.AddScoped<ITerminalRepository, TerminalRepository>();

        // 4. Enregistrement des Repositories Utilisateurs et Abonnés
        services.AddScoped<ISubscriberRepository, SubscriberRepository>();
        services.AddScoped<IOperatorRepository, OperatorRepository>();

        // 5. Enregistrement du Repository des Alertes
        services.AddScoped<IAlertRepository, AlertRepository>();

        return services;
    }
}