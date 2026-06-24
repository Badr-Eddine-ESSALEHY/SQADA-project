namespace ZKTeco.Parking.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateDailyReportPdfAsync(int parkingId, DateTime date);
    Task<byte[]> GenerateDailyReportExcelAsync(int parkingId, DateTime date);
    Task<byte[]> GenerateMonthlyReportPdfAsync(int parkingId, int year, int month);
    Task<byte[]> GenerateMonthlyReportExcelAsync(int parkingId, int year, int month);
}
