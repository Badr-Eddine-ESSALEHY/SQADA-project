using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZKTeco.Parking.Application.Interfaces;
using ZKTeco.Parking.Domain.Interfaces;

namespace ZKTeco.Parking.Application.Services;

public class ReportService : IReportService
{
    private readonly IParkingRecordRepository _recordRepository;
    private readonly IParkingRepository _parkingRepository;

    public ReportService(IParkingRecordRepository recordRepository, IParkingRepository parkingRepository)
    {
        _recordRepository = recordRepository;
        _parkingRepository = parkingRepository;
    }

    public async Task<byte[]> GenerateDailyReportPdfAsync(int parkingId, DateTime date)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);
        var records = (await _recordRepository.GetByDateRangeAsync(parkingId, startOfDay, endOfDay)).ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text($"Daily Parking Report - {parking?.Name ?? "Parking"}").FontSize(18).Bold();
                    col.Item().Text($"Date: {date:dd/MM/yyyy}").FontSize(12);
                    col.Item().Text($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                });

                page.Content().Column(col =>
                {
                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("#").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Plate No").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Card No").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Entry Time").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Exit Time").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Duration").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Amount").Bold();
                        });

                        int rowNum = 1;
                        foreach (var record in records)
                        {
                            var bg = rowNum % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                            table.Cell().Background(bg).Padding(4).Text(rowNum.ToString());
                            table.Cell().Background(bg).Padding(4).Text(record.PlateNo);
                            table.Cell().Background(bg).Padding(4).Text(record.CardNo);
                            table.Cell().Background(bg).Padding(4).Text(record.EntryTime.ToString("HH:mm"));
                            table.Cell().Background(bg).Padding(4).Text(record.ExitTime?.ToString("HH:mm") ?? "-");
                            table.Cell().Background(bg).Padding(4).Text(record.Duration.HasValue ? $"{(int)record.Duration.Value.TotalMinutes}m" : "-");
                            table.Cell().Background(bg).Padding(4).Text(record.Amount.HasValue ? $"{record.Amount:F2}" : "-");
                            rowNum++;
                        }
                    });

                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Total Entries: {records.Count}").Bold();
                            c.Item().Text($"Total Exits: {records.Count(r => r.ExitTime.HasValue)}").Bold();
                            c.Item().Text($"Total Revenue: {records.Where(r => r.Amount.HasValue).Sum(r => r.Amount!.Value):F2}").Bold();
                        });
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateDailyReportExcelAsync(int parkingId, DateTime date)
    {
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);
        var records = (await _recordRepository.GetByDateRangeAsync(parkingId, startOfDay, endOfDay)).ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Daily Report");

        // Title
        worksheet.Cell(1, 1).Value = $"Daily Parking Report - {parking?.Name ?? "Parking"}";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Range(1, 1, 1, 7).Merge();

        worksheet.Cell(2, 1).Value = $"Date: {date:dd/MM/yyyy}";
        worksheet.Range(2, 1, 2, 7).Merge();

        // Headers
        var headers = new[] { "#", "Plate No", "Card No", "Entry Time", "Exit Time", "Duration (min)", "Amount" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Data
        int row = 5;
        foreach (var record in records)
        {
            worksheet.Cell(row, 1).Value = row - 4;
            worksheet.Cell(row, 2).Value = record.PlateNo;
            worksheet.Cell(row, 3).Value = record.CardNo;
            worksheet.Cell(row, 4).Value = record.EntryTime.ToString("HH:mm:ss");
            worksheet.Cell(row, 5).Value = record.ExitTime?.ToString("HH:mm:ss") ?? "-";
            worksheet.Cell(row, 6).Value = record.Duration.HasValue ? (int)record.Duration.Value.TotalMinutes : 0;
            worksheet.Cell(row, 7).Value = record.Amount ?? 0;
            row++;
        }

        // Summary
        row += 2;
        worksheet.Cell(row, 1).Value = "Summary";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        row++;
        worksheet.Cell(row, 1).Value = "Total Entries:";
        worksheet.Cell(row, 2).Value = records.Count;
        row++;
        worksheet.Cell(row, 1).Value = "Total Exits:";
        worksheet.Cell(row, 2).Value = records.Count(r => r.ExitTime.HasValue);
        row++;
        worksheet.Cell(row, 1).Value = "Total Revenue:";
        worksheet.Cell(row, 2).Value = records.Where(r => r.Amount.HasValue).Sum(r => r.Amount!.Value);

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> GenerateMonthlyReportPdfAsync(int parkingId, int year, int month)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var startOfMonth = new DateTime(year, month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);
        var records = (await _recordRepository.GetByDateRangeAsync(parkingId, startOfMonth, endOfMonth)).ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text($"Monthly Parking Report - {parking?.Name ?? "Parking"}").FontSize(18).Bold();
                    col.Item().Text($"Period: {startOfMonth:MMMM yyyy}").FontSize(12);
                    col.Item().Text($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                });

                page.Content().Column(col =>
                {
                    // Daily summary table
                    col.Item().PaddingTop(10).Text("Daily Summary").FontSize(14).Bold();
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Date").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Entries").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Exits").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Revenue").Bold();
                        });

                        for (var d = startOfMonth; d <= endOfMonth.Date; d = d.AddDays(1))
                        {
                            var dayRecords = records.Where(r => r.EntryTime.Date == d).ToList();
                            var bg = d.Day % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                            table.Cell().Background(bg).Padding(4).Text(d.ToString("dd/MM/yyyy"));
                            table.Cell().Background(bg).Padding(4).Text(dayRecords.Count.ToString());
                            table.Cell().Background(bg).Padding(4).Text(dayRecords.Count(r => r.ExitTime.HasValue).ToString());
                            table.Cell().Background(bg).Padding(4).Text(dayRecords.Where(r => r.Amount.HasValue).Sum(r => r.Amount!.Value).ToString("F2"));
                        }
                    });

                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Total Entries: {records.Count}").Bold();
                            c.Item().Text($"Total Exits: {records.Count(r => r.ExitTime.HasValue)}").Bold();
                            c.Item().Text($"Total Revenue: {records.Where(r => r.Amount.HasValue).Sum(r => r.Amount!.Value):F2}").Bold();
                        });
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateMonthlyReportExcelAsync(int parkingId, int year, int month)
    {
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var startOfMonth = new DateTime(year, month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);
        var records = (await _recordRepository.GetByDateRangeAsync(parkingId, startOfMonth, endOfMonth)).ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Monthly Report");

        worksheet.Cell(1, 1).Value = $"Monthly Parking Report - {parking?.Name ?? "Parking"}";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Range(1, 1, 1, 4).Merge();

        worksheet.Cell(2, 1).Value = $"Period: {startOfMonth:MMMM yyyy}";
        worksheet.Range(2, 1, 2, 4).Merge();

        var headers = new[] { "Date", "Entries", "Exits", "Revenue" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 5;
        for (var d = startOfMonth; d <= endOfMonth.Date; d = d.AddDays(1))
        {
            var dayRecords = records.Where(r => r.EntryTime.Date == d).ToList();
            worksheet.Cell(row, 1).Value = d.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 2).Value = dayRecords.Count;
            worksheet.Cell(row, 3).Value = dayRecords.Count(r => r.ExitTime.HasValue);
            worksheet.Cell(row, 4).Value = dayRecords.Where(r => r.Amount.HasValue).Sum(r => r.Amount!.Value);
            row++;
        }

        row += 2;
        worksheet.Cell(row, 1).Value = "Total Entries:";
        worksheet.Cell(row, 2).Value = records.Count;
        row++;
        worksheet.Cell(row, 1).Value = "Total Revenue:";
        worksheet.Cell(row, 2).Value = records.Where(r => r.Amount.HasValue).Sum(r => r.Amount!.Value);

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
