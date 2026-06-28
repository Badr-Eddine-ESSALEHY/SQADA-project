using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using ZKTeco.Parking.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ZKTeco.Parking.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendDailyReportAsync(
        string toEmail,
        string parkingName,
        DateTime date,
        byte[] pdfBytes,
        byte[] excelBytes)
    {
        var smtpHost = _config["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
        var fromEmail = _config["EmailSettings:FromEmail"] ?? "";
        var fromPassword = _config["EmailSettings:FromPassword"] ?? "";
        var fromName = _config["EmailSettings:FromName"] ?? "ZKTeco Parking";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = $"Rapport Journalier — {parkingName} — {date:dd/MM/yyyy}";

        var builder = new BodyBuilder();
        builder.HtmlBody = $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: #00467F; padding: 20px; border-radius: 8px 8px 0 0;'>
        <h2 style='color: white; margin: 0;'>ZKTeco Parking Dashboard</h2>
        <p style='color: #cce4ff; margin: 5px 0 0 0;'>{parkingName}</p>
    </div>
    <div style='background: #f5f5f5; padding: 20px; border-radius: 0 0 8px 8px;'>
        <h3 style='color: #00467F;'>Rapport Journalier — {date:dd MMMM yyyy}</h3>
        <p>Veuillez trouver ci-joint le rapport journalier du parking <strong>{parkingName}</strong> 
        pour la date du <strong>{date:dd/MM/yyyy}</strong>.</p>
        <p>Deux fichiers sont joints à cet email :</p>
        <ul>
            <li>📄 <strong>PDF</strong> — Rapport détaillé avec KPIs et statistiques</li>
            <li>📊 <strong>Excel</strong> — Données brutes pour analyse</li>
        </ul>
        <hr style='border: 1px solid #ddd; margin: 20px 0;'/>
        <p style='color: #888; font-size: 12px;'>
            Ce rapport est généré automatiquement chaque jour à minuit.<br/>
            Compagnie Générale des Parkings — ZKTeco Dashboard v1.0
        </p>
    </div>
</div>";

        builder.Attachments.Add(
            $"rapport-journalier-{parkingName}-{date:yyyy-MM-dd}.pdf",
            pdfBytes,
            ContentType.Parse("application/pdf"));

        builder.Attachments.Add(
            $"rapport-journalier-{parkingName}-{date:yyyy-MM-dd}.xlsx",
            excelBytes,
            ContentType.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(fromEmail, fromPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}