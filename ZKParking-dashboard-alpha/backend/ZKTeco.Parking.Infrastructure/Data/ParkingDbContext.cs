using Microsoft.EntityFrameworkCore;
using ZKTeco.Parking.Domain.Entities;

namespace ZKTeco.Parking.Infrastructure.Data;

public class ParkingDbContext : DbContext
{
    public ParkingDbContext(DbContextOptions<ParkingDbContext> options) : base(options) { }

    public DbSet<Domain.Entities.Parking> Parkings { get; set; } = null!;
    public DbSet<ParkingRecord> ParkingRecords { get; set; } = null!;
    public DbSet<Subscriber> Subscribers { get; set; } = null!;
    public DbSet<Operator> Operators { get; set; } = null!;
    public DbSet<Gate> Gates { get; set; } = null!;
    public DbSet<Terminal> Terminals { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Alert> Alerts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Parking
        modelBuilder.Entity<Domain.Entities.Parking>(entity =>
        {
            entity.ToTable("parking");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).HasColumnName("code").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(500);
            entity.Property(e => e.TotalSpaces).HasColumnName("total_spaces");
            entity.Property(e => e.HourlyRate).HasColumnName("hourly_rate").HasColumnType("decimal(10,2)");
            entity.Property(e => e.DailyRate).HasColumnName("daily_rate").HasColumnType("decimal(10,2)");
            entity.Property(e => e.MonthlyRate).HasColumnName("monthly_rate").HasColumnType("decimal(10,2)");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        });

        // ParkingRecord
        modelBuilder.Entity<ParkingRecord>(entity =>
        {
            entity.ToTable("parking_record");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.CardNo).HasColumnName("card_no").IsRequired().HasMaxLength(100);
            entity.Property(e => e.PlateNo).HasColumnName("plate_no").IsRequired().HasMaxLength(50);
            entity.Property(e => e.EntryTime).HasColumnName("entry_time");
            entity.Property(e => e.ExitTime).HasColumnName("exit_time");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
            entity.Property(e => e.TicketType).HasColumnName("ticket_type").HasMaxLength(50);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.ParkingId).HasColumnName("parking_id");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.GateInId).HasColumnName("gate_in_id");
            entity.Property(e => e.GateOutId).HasColumnName("gate_out_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Parking)
                .WithMany(p => p.ParkingRecords)
                .HasForeignKey(e => e.ParkingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Operator)
                .WithMany(o => o.ParkingRecords)
                .HasForeignKey(e => e.OperatorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.GateIn)
                .WithMany(g => g.EntryRecords)
                .HasForeignKey(e => e.GateInId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.GateOut)
                .WithMany(g => g.ExitRecords)
                .HasForeignKey(e => e.GateOutId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.CardNo);
            entity.HasIndex(e => e.PlateNo);
            entity.HasIndex(e => e.EntryTime);
            entity.HasIndex(e => new { e.ParkingId, e.EntryTime });
        });

        // Subscriber
        modelBuilder.Entity<Subscriber>(entity =>
        {
            entity.ToTable("subscriber");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200);
            entity.Property(e => e.PlateNo).HasColumnName("plate_no").HasMaxLength(50);
            entity.Property(e => e.CardNo).HasColumnName("card_no").HasMaxLength(100);
            entity.Property(e => e.SubscriptionType).HasColumnName("subscription_type").HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
            entity.Property(e => e.ParkingId).HasColumnName("parking_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Parking)
                .WithMany(p => p.Subscribers)
                .HasForeignKey(e => e.ParkingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Operator
        modelBuilder.Entity<Operator>(entity =>
        {
            entity.ToTable("operator");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Username).HasColumnName("username").IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(200);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50);
            entity.Property(e => e.ParkingIds).HasColumnName("parking_ids").HasMaxLength(500);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.LastLogin).HasColumnName("last_login");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.RefreshToken).HasColumnName("refresh_token");
            entity.Property(e => e.RefreshTokenExpiry).HasColumnName("refresh_token_expiry");

            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Gate
        modelBuilder.Entity<Gate>(entity =>
        {
            entity.ToTable("gate");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.ParkingId).HasColumnName("parking_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.LastPing).HasColumnName("last_ping");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);

            entity.HasOne(e => e.Parking)
                .WithMany(p => p.Gates)
                .HasForeignKey(e => e.ParkingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Terminal
        modelBuilder.Entity<Terminal>(entity =>
        {
            entity.ToTable("terminal");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.ParkingId).HasColumnName("parking_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.LastPing).HasColumnName("last_ping");
            entity.Property(e => e.FirmwareVersion).HasColumnName("firmware_version").HasMaxLength(50);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);

            entity.HasOne(e => e.Parking)
                .WithMany(p => p.Terminals)
                .HasForeignKey(e => e.ParkingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payment");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.ParkingRecordId).HasColumnName("parking_record_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
            entity.Property(e => e.PaidAt).HasColumnName("paid_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);

            entity.HasOne(e => e.ParkingRecord)
                .WithMany(r => r.Payments)
                .HasForeignKey(e => e.ParkingRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Operator)
                .WithMany()
                .HasForeignKey(e => e.OperatorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Alert
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.ToTable("alert");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.ParkingId).HasColumnName("parking_id");
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(100);
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Severity).HasColumnName("severity").HasMaxLength(50);
            entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");

            entity.HasOne(e => e.Parking)
                .WithMany(p => p.Alerts)
                .HasForeignKey(e => e.ParkingId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
