using Microsoft.EntityFrameworkCore;
using ZKTeco.Parking.Domain.Entities;
using ZKTeco.Parking.Infrastructure.Data;

namespace ZKTeco.Parking.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ParkingDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        // Seed Parking
        if (!await context.Parkings.AnyAsync())
        {
            var parking = new Domain.Entities.Parking
            {
                Name = "Parking Central",
                Code = "PKG-001",
                Address = "123 Main Street, City Center",
                TotalSpaces = 200,
                HourlyRate = 5.00m,
                DailyRate = 30.00m,
                MonthlyRate = 500.00m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Parkings.Add(parking);
            await context.SaveChangesAsync();

            // Seed Gates
            var gates = new[]
            {
                new Gate { ParkingId = parking.Id, Name = "Entry Gate A", Type = "Entry", IpAddress = "192.168.1.10", Status = "Online", LastPing = DateTime.UtcNow, IsActive = true },
                new Gate { ParkingId = parking.Id, Name = "Entry Gate B", Type = "Entry", IpAddress = "192.168.1.11", Status = "Online", LastPing = DateTime.UtcNow, IsActive = true },
                new Gate { ParkingId = parking.Id, Name = "Exit Gate A", Type = "Exit", IpAddress = "192.168.1.20", Status = "Online", LastPing = DateTime.UtcNow, IsActive = true },
                new Gate { ParkingId = parking.Id, Name = "Exit Gate B", Type = "Exit", IpAddress = "192.168.1.21", Status = "Offline", LastPing = DateTime.UtcNow.AddHours(-2), IsActive = true }
            };
            context.Gates.AddRange(gates);

            // Seed Terminals
            var terminals = new[]
            {
                new Terminal { ParkingId = parking.Id, Name = "Cashier Terminal 1", Type = "Cashier", IpAddress = "192.168.1.30", Status = "Online", LastPing = DateTime.UtcNow, FirmwareVersion = "v2.1.0", IsActive = true },
                new Terminal { ParkingId = parking.Id, Name = "Cashier Terminal 2", Type = "Cashier", IpAddress = "192.168.1.31", Status = "Online", LastPing = DateTime.UtcNow, FirmwareVersion = "v2.1.0", IsActive = true },
                new Terminal { ParkingId = parking.Id, Name = "Kiosk Terminal 1", Type = "Kiosk", IpAddress = "192.168.1.40", Status = "Online", LastPing = DateTime.UtcNow, FirmwareVersion = "v1.5.2", IsActive = true }
            };
            context.Terminals.AddRange(terminals);

            await context.SaveChangesAsync();
        }

        // Seed Admin Operator
        if (!await context.Operators.AnyAsync())
        {
            var admin = new Operator
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "System Administrator",
                Email = "admin@zkteco-parking.com",
                Phone = "+1234567890",
                Role = "Admin",
                ParkingIds = "1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var supervisor = new Operator
            {
                Username = "supervisor",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Supervisor@123"),
                FullName = "Parking Supervisor",
                Email = "supervisor@zkteco-parking.com",
                Phone = "+1234567891",
                Role = "Supervisor",
                ParkingIds = "1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var cashier = new Operator
            {
                Username = "cashier1",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cashier@123"),
                FullName = "John Cashier",
                Email = "cashier1@zkteco-parking.com",
                Phone = "+1234567892",
                Role = "Cashier",
                ParkingIds = "1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Operators.AddRange(admin, supervisor, cashier);
            await context.SaveChangesAsync();
        }

        // Seed sample subscribers
        if (!await context.Subscribers.AnyAsync())
        {
            var parkingId = (await context.Parkings.FirstAsync()).Id;
            var subscribers = new[]
            {
                new Subscriber { Name = "Alice Johnson", Phone = "555-0101", Email = "alice@example.com", PlateNo = "ABC-1234", CardNo = "CARD-001", SubscriptionType = "Monthly", StartDate = DateTime.UtcNow.AddDays(-15), EndDate = DateTime.UtcNow.AddDays(15), Status = "Active", Amount = 500.00m, ParkingId = parkingId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Subscriber { Name = "Bob Smith", Phone = "555-0102", Email = "bob@example.com", PlateNo = "XYZ-5678", CardNo = "CARD-002", SubscriptionType = "Monthly", StartDate = DateTime.UtcNow.AddDays(-25), EndDate = DateTime.UtcNow.AddDays(5), Status = "Active", Amount = 500.00m, ParkingId = parkingId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Subscriber { Name = "Carol White", Phone = "555-0103", Email = "carol@example.com", PlateNo = "DEF-9012", CardNo = "CARD-003", SubscriptionType = "Annual", StartDate = DateTime.UtcNow.AddMonths(-6), EndDate = DateTime.UtcNow.AddMonths(6), Status = "Active", Amount = 5000.00m, ParkingId = parkingId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            context.Subscribers.AddRange(subscribers);
            await context.SaveChangesAsync();
        }

        // Seed sample alerts
        if (!await context.Alerts.AnyAsync())
        {
            var parkingId = (await context.Parkings.FirstAsync()).Id;
            var alerts = new[]
            {
                new Alert { ParkingId = parkingId, Type = "GateOffline", Message = "Exit Gate B is offline", Severity = "Warning", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-1) },
                new Alert { ParkingId = parkingId, Type = "HighOccupancy", Message = "Parking occupancy above 90%", Severity = "Info", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-3) },
                new Alert { ParkingId = parkingId, Type = "SubscriptionExpiring", Message = "3 subscriptions expiring within 7 days", Severity = "Info", IsRead = true, CreatedAt = DateTime.UtcNow.AddDays(-1), ReadAt = DateTime.UtcNow.AddHours(-12) }
            };
            context.Alerts.AddRange(alerts);
            await context.SaveChangesAsync();
        }
    }
}
