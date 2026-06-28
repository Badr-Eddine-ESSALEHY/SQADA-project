namespace ZKTeco.Parking.Application.Interfaces;

public interface IEmailService
{
    Task SendDailyReportAsync(
        string toEmail,
        string parkingName,
        DateTime date,
        byte[] pdfBytes,
        byte[] excelBytes);
}