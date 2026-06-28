namespace ZKTeco.Parking.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateDailyReportPdfAsync(int parkingId, DateTime date);
    Task<byte[]> GenerateDailyReportExcelAsync(int parkingId, DateTime date);
    Task<byte[]> GenerateMonthlyReportPdfAsync(int parkingId, int year, int month);
    Task<byte[]> GenerateMonthlyReportExcelAsync(int parkingId, int year, int month);
    Task<byte[]> GenerateWeeklyReportPdfAsync(int parkingId, DateTime weekStart);
    Task<byte[]> GenerateWeeklyReportExcelAsync(int parkingId, DateTime weekStart);
    Task<byte[]> GenerateAnnualReportPdfAsync(int parkingId, int year);
    Task<byte[]> GenerateAnnualReportExcelAsync(int parkingId, int year);
}