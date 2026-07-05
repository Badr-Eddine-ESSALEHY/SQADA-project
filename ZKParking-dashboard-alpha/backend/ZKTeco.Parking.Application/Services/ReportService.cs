using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZKTeco.Parking.Application.Interfaces;
using ZKTeco.Parking.Domain.Entities;
using ZKTeco.Parking.Domain.Interfaces;

namespace ZKTeco.Parking.Application.Services;

public class ReportService : IReportService
{
    private readonly IParkingRecordRepository _recordRepository;
    private readonly IParkingRepository _parkingRepository;
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly IOperatorRepository _operatorRepository;

    public ReportService(
        IParkingRecordRepository recordRepository,
        IParkingRepository parkingRepository,
        ISubscriberRepository subscriberRepository,
        IOperatorRepository operatorRepository)
    {
        _recordRepository = recordRepository;
        _parkingRepository = parkingRepository;
        _subscriberRepository = subscriberRepository;
        _operatorRepository = operatorRepository;
    }

    // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private static int NightEntries(List<ParkingRecord> r) =>
        r.Count(x => x.EntryTime.Hour < 6 || x.EntryTime.Hour >= 20);
    private static int DayEntries(List<ParkingRecord> r) => r.Count - NightEntries(r);
    private static int DayExits(List<ParkingRecord> r) =>
        r.Count(x => x.ExitTime.HasValue && x.ExitTime.Value.Hour >= 6 && x.ExitTime.Value.Hour < 20);
    private static int NightExits(List<ParkingRecord> r) =>
        r.Count(x => x.ExitTime.HasValue && (x.ExitTime.Value.Hour < 6 || x.ExitTime.Value.Hour >= 20));
    private static int LostTickets(List<ParkingRecord> r) =>
        r.Count(x => x.Status == "Perdu" || x.Status == "Lost");
    private static int UnreadableTickets(List<ParkingRecord> r) =>
        r.Count(x => x.Status == "Illisible" || x.Status == "Unreadable");
    private static decimal TotalRevenue(List<ParkingRecord> r) =>
        r.Where(x => x.Amount.HasValue).Sum(x => x.Amount!.Value);
    private static double AvgDuration(List<ParkingRecord> r)
    {
        var c = r.Where(x => x.Duration.HasValue).ToList();
        return c.Any() ? c.Average(x => x.Duration!.Value.TotalMinutes) : 0;
    }

    // â”€â”€ PDF Builder â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private static IDocument BuildPdf(
        string parkingName,
        string title,
        string period,
        List<ParkingRecord> records,
        int totalSpaces,
        int activeSubscribers,
        Action<ColumnDescriptor> extraContent)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("ZKTeco Parking Dashboard")
                                .FontSize(9).FontColor(Colors.Grey.Medium);
                            c.Item().Text(parkingName)
                                .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                            c.Item().Text(title).FontSize(13).Bold();
                        });
                        row.ConstantItem(130).AlignRight().Column(c =>
                        {
                            c.Item().Text($"Généré le: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(period).FontSize(9).Bold();
                        });
                    });
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
                });

                // Content
                page.Content().Column(col =>
                {
                    // KPI summary table
                    col.Item().PaddingTop(10).Text("Résumé des KPI").FontSize(12).Bold();
                    col.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                        });

                        static IContainer HdrStyle(IContainer c) =>
                            c.Background(Colors.Blue.Darken2).Padding(5);

                        table.Header(h =>
                        {
                            h.Cell().Element(HdrStyle).Text("Parking").FontColor(Colors.White).Bold().FontSize(9);
                            h.Cell().Element(HdrStyle).Text("N.E Jour").FontColor(Colors.White).Bold().FontSize(9);
                            h.Cell().Element(HdrStyle).Text("N.E Nuit").FontColor(Colors.White).Bold().FontSize(9);
                            h.Cell().Element(HdrStyle).Text("NS Jour").FontColor(Colors.White).Bold().FontSize(9);
                            h.Cell().Element(HdrStyle).Text("NS Nuit").FontColor(Colors.White).Bold().FontSize(9);
                            h.Cell().Element(HdrStyle).Text("Tk.Perdus").FontColor(Colors.White).Bold().FontSize(9);
                            h.Cell().Element(HdrStyle).Text("Tk.Illisibles").FontColor(Colors.White).Bold().FontSize(9);
                        });

                        static IContainer DataStyle(IContainer c) =>
                            c.Background(Colors.Grey.Lighten3).Padding(5);

                        table.Cell().Element(DataStyle).Text("Total").FontSize(9);
                        table.Cell().Element(DataStyle).Text(DayEntries(records).ToString()).FontSize(9);
                        table.Cell().Element(DataStyle).Text(NightEntries(records).ToString()).FontSize(9);
                        table.Cell().Element(DataStyle).Text(DayExits(records).ToString()).FontSize(9);
                        table.Cell().Element(DataStyle).Text(NightExits(records).ToString()).FontSize(9);
                        table.Cell().Element(DataStyle).Text(LostTickets(records).ToString()).FontSize(9);
                        table.Cell().Element(DataStyle).Text(UnreadableTickets(records).ToString()).FontSize(9);
                    });

                    // KPI cards
                    var revenue = TotalRevenue(records);
                    var currentOcc = records.Count(r => !r.ExitTime.HasValue);
                    var occRate = totalSpaces > 0 ? (double)currentOcc / totalSpaces * 100 : 0;
                    var avgDur = AvgDuration(records);

                    col.Item().PaddingTop(10).Row(row =>
                    {
                        static IContainer CardStyle(IContainer c) =>
                            c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8);

                        row.RelativeItem().Element(CardStyle).Column(c =>
                        {
                            c.Item().Text("Chiffre d'Affaires").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text($"{revenue:F2} Dh").FontSize(14).Bold().FontColor(Colors.Green.Darken1);
                        });
                        row.ConstantItem(8);
                        row.RelativeItem().Element(CardStyle).Column(c =>
                        {
                            c.Item().Text("Taux d'Occupation").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text($"{occRate:F1} %").FontSize(14).Bold().FontColor(Colors.Blue.Darken1);
                        });
                        row.ConstantItem(8);
                        row.RelativeItem().Element(CardStyle).Column(c =>
                        {
                            c.Item().Text("Durée Moyenne").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text($"{(int)avgDur} min").FontSize(14).Bold().FontColor(Colors.Orange.Darken1);
                        });
                        row.ConstantItem(8);
                        row.RelativeItem().Element(CardStyle).Column(c =>
                        {
                            c.Item().Text("Abonnés Actifs").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(activeSubscribers.ToString()).FontSize(14).Bold().FontColor(Colors.Purple.Darken1);
                        });
                    });

                    // Ticket type breakdown
                    col.Item().PaddingTop(12).Text("Détail par type de ticket").FontSize(11).Bold();
                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });

                        static IContainer Hdr2(IContainer c) =>
                            c.Background(Colors.Grey.Lighten2).Padding(5);

                        table.Header(h =>
                        {
                            h.Cell().Element(Hdr2).Text("Type de tickets").Bold().FontSize(9);
                            h.Cell().Element(Hdr2).Text("Nombre").Bold().FontSize(9);
                            h.Cell().Element(Hdr2).Text("Montant total (Dh)").Bold().FontSize(9);
                        });

                        bool alt = false;
                        foreach (var g in records.GroupBy(r => r.TicketType).OrderBy(g => g.Key))
                        {
                            string bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                            table.Cell().Background(bg).Padding(4).Text(g.Key).FontSize(9);
                            table.Cell().Background(bg).Padding(4).Text(g.Count().ToString()).FontSize(9);
                            table.Cell().Background(bg).Padding(4)
                                .Text(g.Where(r => r.Amount.HasValue).Sum(r => r.Amount!.Value).ToString("F2")).FontSize(9);
                            alt = !alt;
                        }

                        table.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Total").Bold().FontSize(9);
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(records.Count.ToString()).Bold().FontSize(9);
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text($"{revenue:F2}").Bold().FontSize(9);
                    });

                    // Operator section
                    var byOp = records.Where(r => r.Operator != null)
                        .GroupBy(r => r.Operator!.FullName).ToList();

                    if (byOp.Any())
                    {
                        col.Item().PaddingTop(14).Text("Détail par Opérateur / Caissier").FontSize(11).Bold();
                        col.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                            });

                            static IContainer OpHdr(IContainer c) =>
                                c.Background(Colors.Grey.Lighten2).Padding(5);

                            table.Header(h =>
                            {
                                h.Cell().Element(OpHdr).Text("Opérateur").Bold().FontSize(9);
                                h.Cell().Element(OpHdr).Text("Transactions").Bold().FontSize(9);
                                h.Cell().Element(OpHdr).Text("Sorties").Bold().FontSize(9);
                                h.Cell().Element(OpHdr).Text("CA (Dh)").Bold().FontSize(9);
                            });

                            bool alt = false;
                            foreach (var g in byOp)
                            {
                                string bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                                table.Cell().Background(bg).Padding(4).Text(g.Key).FontSize(9);
                                table.Cell().Background(bg).Padding(4).Text(g.Count().ToString()).FontSize(9);
                                table.Cell().Background(bg).Padding(4)
                                    .Text(g.Count(r => r.ExitTime.HasValue).ToString()).FontSize(9);
                                table.Cell().Background(bg).Padding(4)
                                    .Text(g.Where(r => r.Amount.HasValue).Sum(r => r.Amount!.Value).ToString("F2")).FontSize(9);
                                alt = !alt;
                            }
                        });
                    }

                    // Extra content (daily stats, weekly table, etc.)
                    extraContent(col);
                });

                // Footer
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text("Compagnie Générale des Parkings")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                    row.RelativeItem().AlignCenter().Text(x =>
                    {
                        x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        x.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                    row.RelativeItem().AlignRight()
                        .Text(DateTime.Now.ToString("dd/MM/yyyy"))
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });
    }

    // â”€â”€ Stats table helper â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private static void AddStatsTable(ColumnDescriptor col,
        List<ParkingRecord> records, int totalSpaces)
    {
        col.Item().PaddingTop(14).Text("Statistiques du Parking").FontSize(11).Bold();
        col.Item().PaddingTop(4).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3);
                c.RelativeColumn(2);
            });

            void StatRow(string label, string value)
            {
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4)
                    .Text(label).FontSize(9);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4)
                    .Text(value).FontSize(9).Bold();
            }

            var avgDur = AvgDuration(records);
            StatRow("Capacité du parking (CP)", totalSpaces.ToString());
            StatRow("Nombre des Entrées (NE)", records.Count.ToString());
            StatRow("dont abonnés", records.Count(r => r.TicketType == "Abonnement").ToString());
            StatRow("dont visiteurs", records.Count(r => r.TicketType != "Abonnement").ToString());
            StatRow("Nombre des Sorties (NS)", records.Count(r => r.ExitTime.HasValue).ToString());
            StatRow("Tickets < 15min", records.Count(r => r.Duration.HasValue && r.Duration.Value.TotalMinutes < 15).ToString());
            StatRow("Chiffre d'Affaire (Dh)", $"{TotalRevenue(records):F2}");
            StatRow("Ticket Moyen (Dh)", records.Any(r => r.Amount.HasValue)
                ? $"{records.Where(r => r.Amount.HasValue).Average(r => r.Amount!.Value):F2}" : "0.00");
            StatRow("Ratio de Rotation (NE/CP)", totalSpaces > 0
                ? $"{(double)records.Count / totalSpaces:F2}" : "0.00");
            StatRow("Taux d'occupation moyen",
                $"{(totalSpaces > 0 ? (double)records.Count(r => !r.ExitTime.HasValue) / totalSpaces * 100 : 0):F2} %");
            StatRow("Durée moyenne de stationnement",
                $"{(int)avgDur / 60} H : {(int)avgDur % 60} min");
        });
    }

    // â”€â”€ Daily PDF â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<byte[]> GenerateDailyReportPdfAsync(int parkingId, DateTime date)
    {
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var records = (await _recordRepository.GetByDateRangeAsync(
            parkingId, date.Date, date.Date.AddDays(1).AddTicks(-1))).ToList();
        var subscribers = (await _subscriberRepository.GetAllAsync())
            .Count(s => s.ParkingId == parkingId && s.Status == "Active");
        var totalSpaces = parking?.TotalSpaces ?? 100;

        return BuildPdf(
            parking?.Name ?? "Parking",
            "Rapport Journalier — C.A Visiteurs",
            $"Chiffre d'affaires du {date:dd/MM/yyyy}",
            records, totalSpaces, subscribers,
            col => AddStatsTable(col, records, totalSpaces)
        ).GeneratePdf();
    }

    //  Weekly PDF 

    public async Task<byte[]> GenerateWeeklyReportPdfAsync(int parkingId, DateTime weekStart)
    {
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var start = weekStart.Date;
        var end = start.AddDays(7).AddTicks(-1);
        var records = (await _recordRepository.GetByDateRangeAsync(parkingId, start, end)).ToList();
        var subscribers = (await _subscriberRepository.GetAllAsync())
            .Count(s => s.ParkingId == parkingId && s.Status == "Active");
        var totalSpaces = parking?.TotalSpaces ?? 100;

        return BuildPdf(
            parking?.Name ?? "Parking",
            "Rapport Hebdomadaire",
            $"Semaine du {start:dd/MM/yyyy} au {end:dd/MM/yyyy}",
            records, totalSpaces, subscribers,
            col =>
            {
                col.Item().PaddingTop(14).Text("Détail Journalier").FontSize(11).Bold();
                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                    });

                    static IContainer Hdr(IContainer c) =>
                        c.Background(Colors.Grey.Lighten2).Padding(5);

                    table.Header(h =>
                    {
                        h.Cell().Element(Hdr).Text("Date").Bold().FontSize(9);
                        h.Cell().Element(Hdr).Text("Entrées").Bold().FontSize(9);
                        h.Cell().Element(Hdr).Text("Sorties").Bold().FontSize(9);
                        h.Cell().Element(Hdr).Text("Tk.Perdus").Bold().FontSize(9);
                        h.Cell().Element(Hdr).Text("CA (Dh)").Bold().FontSize(9);
                    });

                    bool alt = false;
                    for (var d = start; d <= end.Date; d = d.AddDays(1))
                    {
                        var dr = records.Where(r => r.EntryTime.Date == d).ToList();
                        string bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                        table.Cell().Background(bg).Padding(4).Text(d.ToString("ddd dd/MM")).FontSize(9);
                        table.Cell().Background(bg).Padding(4).Text(dr.Count.ToString()).FontSize(9);
                        table.Cell().Background(bg).Padding(4).Text(dr.Count(r => r.ExitTime.HasValue).ToString()).FontSize(9);
                        table.Cell().Background(bg).Padding(4).Text(LostTickets(dr).ToString()).FontSize(9);
                        table.Cell().Background(bg).Padding(4).Text($"{TotalRevenue(dr):F2}").FontSize(9);
                        alt = !alt;
                    }
                });
            }
        ).GeneratePdf();
    }

    // â”€â”€ Monthly PDF â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<byte[]> GenerateMonthlyReportPdfAsync(int parkingId, int year, int month)
    {
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1).AddTicks(-1);
        var records = (await _recordRepository.GetByDateRangeAsync(parkingId, start, end)).ToList();
        var subscribers = (await _subscriberRepository.GetAllAsync())
            .Count(s => s.ParkingId == parkingId && s.Status == "Active");
        var totalSpaces = parking?.TotalSpaces ?? 100;

        return BuildPdf(
            parking?.Name ?? "Parking",
            "Rapport Mensuel",
            $"{start.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))}",
            records, totalSpaces, subscribers,
            col =>
            {
                col.Item().PaddingTop(14).Text("Résumé Journalier").FontSize(11).Bold();
                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                    });

                    static IContainer Hdr(IContainer c) =>
                        c.Background(Colors.Grey.Lighten2).Padding(5);

                    table.Header(h =>
                    {
                        h.Cell().Element(Hdr).Text("Date").Bold().FontSize(9);
                        h.Cell().Element(Hdr).Text("Entrées").Bold().FontSize(9);
                        h.Cell().Element(Hdr).Text("Sorties").Bold().FontSize(9);
                        h.Cell().Element(Hdr).Text("Tk.Perdus").Bold().FontSize(9);
                        h.Cell().Element(Hdr).Text("CA (Dh)").Bold().FontSize(9);
                    });

                    bool alt = false;
                    for (var d = start; d <= end.Date; d = d.AddDays(1))
                    {
                        var dr = records.Where(r => r.EntryTime.Date == d).ToList();
                        string bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                        table.Cell().Background(bg).Padding(4).Text(d.ToString("dd/MM/yyyy")).FontSize(9);
                        table.Cell().Background(bg).Padding(4).Text(dr.Count.ToString()).FontSize(9);
                        table.Cell().Background(bg).Padding(4).Text(dr.Count(r => r.ExitTime.HasValue).ToString()).FontSize(9);
                        table.Cell().Background(bg).Padding(4).Text(LostTickets(dr).ToString()).FontSize(9);
                        table.Cell().Background(bg).Padding(4).Text($"{TotalRevenue(dr):F2}").FontSize(9);
                        alt = !alt;
                    }
                });
            }
        ).GeneratePdf();
    }

    // â”€â”€ Annual PDF â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<byte[]> GenerateAnnualReportPdfAsync(int parkingId, int year)
    {
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var start = new DateTime(year, 1, 1);
        var end = new DateTime(year, 12, 31, 23, 59, 59);
        var records = (await _recordRepository.GetByDateRangeAsync(parkingId, start, end)).ToList();
        var subscribers = (await _subscriberRepository.GetAllAsync())
            .Count(s => s.ParkingId == parkingId && s.Status == "Active");
        var totalSpaces = parking?.TotalSpaces ?? 100;

        return BuildPdf(
            parking?.Name ?? "Parking",
            "Rapport Annuel",
            $"Année {year}",
            records, totalSpaces, subscribers,
            col =>
            {
                col.Item().PaddingTop(14).Text("Résumé Mensuel").FontSize(11).Bold();
                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                    });

                    static IContainer Hdr(IContainer c) =>
                        c.Background(Colors.Grey.Lighten2).Padding(5);

                    table.Header(h =>
                    {
                        h.Cell().Element(Hdr).Text("Mois").Bold().FontSize(9);
                        h.Cell().Element(Hdr).Text("Entrées").Bold().FontSize(9);
                        h.Cell().Element(Hdr).Text("Sorties").Bold().FontSize(9);
                        h.Cell().Element(Hdr).Text("CA (Dh)").Bold().FontSize(9);
                    });

                    bool alt = false;
                    for (int m = 1; m <= 12; m++)
                    {
                        var mr = records.Where(r => r.EntryTime.Month == m).ToList();
                        string bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                        table.Cell().Background(bg).Padding(4)
                            .Text(new DateTime(year, m, 1).ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))).FontSize(9);
                        table.Cell().Background(bg).Padding(4).Text(mr.Count.ToString()).FontSize(9);
                        table.Cell().Background(bg).Padding(4).Text(mr.Count(r => r.ExitTime.HasValue).ToString()).FontSize(9);
                        table.Cell().Background(bg).Padding(4).Text($"{TotalRevenue(mr):F2}").FontSize(9);
                        alt = !alt;
                    }
                });
            }
        ).GeneratePdf();
    }

    // â”€â”€ Excel helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private static void AddKpiSheet(XLWorkbook wb, List<ParkingRecord> records,
        string parkingName, string period, int totalSpaces, int subscribers)
    {
        var ws = wb.Worksheets.Add("Résumé KPI");
        ws.Cell(1, 1).Value = parkingName;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromArgb(0, 70, 127);
        ws.Range(1, 1, 1, 7).Merge();
        ws.Cell(2, 1).Value = period;
        ws.Range(2, 1, 2, 7).Merge();

        int r = 4;
        string[] kpiH = { "Parking", "N.E Jour", "N.E Nuit", "NS Jour", "NS Nuit", "Tk.Perdus", "Tk.Illisibles" };
        for (int i = 0; i < kpiH.Length; i++)
        {
            ws.Cell(r, i + 1).Value = kpiH[i];
            ws.Cell(r, i + 1).Style.Font.Bold = true;
            ws.Cell(r, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 70, 127);
            ws.Cell(r, i + 1).Style.Font.FontColor = XLColor.White;
        }
        r++;
        ws.Cell(r, 1).Value = "Total";
        ws.Cell(r, 2).Value = DayEntries(records);
        ws.Cell(r, 3).Value = NightEntries(records);
        ws.Cell(r, 4).Value = DayExits(records);
        ws.Cell(r, 5).Value = NightExits(records);
        ws.Cell(r, 6).Value = LostTickets(records);
        ws.Cell(r, 7).Value = UnreadableTickets(records);
        ws.Row(r).Style.Fill.BackgroundColor = XLColor.LightGray;

        r += 2;
        ws.Cell(r, 1).Value = "Chiffre d'Affaires Global (Dh)"; ws.Cell(r, 1).Style.Font.Bold = true;
        ws.Cell(r, 2).Value = (double)TotalRevenue(records);
        r++;
        ws.Cell(r, 1).Value = "Taux d'Occupation (%)";
        ws.Cell(r, 2).Value = totalSpaces > 0
            ? Math.Round((double)records.Count(x => !x.ExitTime.HasValue) / totalSpaces * 100, 2) : 0;
        r++;
        ws.Cell(r, 1).Value = "Durée Moyenne (min)";
        ws.Cell(r, 2).Value = Math.Round(AvgDuration(records), 1);
        r++;
        ws.Cell(r, 1).Value = "Abonnés Actifs";
        ws.Cell(r, 2).Value = subscribers;

        r += 2;
        ws.Cell(r, 1).Value = "Type de tickets";
        ws.Cell(r, 2).Value = "Nombre";
        ws.Cell(r, 3).Value = "Montant (Dh)";
        ws.Row(r).Style.Font.Bold = true;
        ws.Row(r).Style.Fill.BackgroundColor = XLColor.LightGray;
        r++;
        foreach (var g in records.GroupBy(x => x.TicketType).OrderBy(g => g.Key))
        {
            ws.Cell(r, 1).Value = g.Key;
            ws.Cell(r, 2).Value = g.Count();
            ws.Cell(r, 3).Value = (double)g.Where(x => x.Amount.HasValue).Sum(x => x.Amount!.Value);
            r++;
        }
        ws.Columns().AdjustToContents();
    }

    private static void AddOperatorSheet(XLWorkbook wb, List<ParkingRecord> records)
    {
        var ws = wb.Worksheets.Add("Détail Opérateurs");
        ws.Cell(1, 1).Value = "Détail par Opérateur / Caissier";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Range(1, 1, 1, 4).Merge();
        int r = 3;
        string[] h = { "Opérateur", "Transactions", "Sorties", "CA (Dh)" };
        for (int i = 0; i < h.Length; i++)
        {
            ws.Cell(r, i + 1).Value = h[i];
            ws.Cell(r, i + 1).Style.Font.Bold = true;
            ws.Cell(r, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        r++;
        foreach (var g in records.Where(x => x.Operator != null).GroupBy(x => x.Operator!.FullName))
        {
            ws.Cell(r, 1).Value = g.Key;
            ws.Cell(r, 2).Value = g.Count();
            ws.Cell(r, 3).Value = g.Count(x => x.ExitTime.HasValue);
            ws.Cell(r, 4).Value = (double)g.Where(x => x.Amount.HasValue).Sum(x => x.Amount!.Value);
            r++;
        }
        ws.Columns().AdjustToContents();
    }

    private static void AddTransactionsSheet(XLWorkbook wb, List<ParkingRecord> records)
    {
        var ws = wb.Worksheets.Add("Transactions");
        string[] h = { "#", "Plaque", "Carte", "Entrée", "Sortie", "Durée (min)", "Montant (Dh)", "Type", "Statut" };
        for (int i = 0; i < h.Length; i++)
        {
            ws.Cell(1, i + 1).Value = h[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 70, 127);
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }
        int r = 2;
        foreach (var rec in records)
        {
            ws.Cell(r, 1).Value = r - 1;
            ws.Cell(r, 2).Value = rec.PlateNo;
            ws.Cell(r, 3).Value = rec.CardNo;
            ws.Cell(r, 4).Value = rec.EntryTime.ToString("HH:mm:ss");
            ws.Cell(r, 5).Value = rec.ExitTime?.ToString("HH:mm:ss") ?? "-";
            ws.Cell(r, 6).Value = rec.Duration.HasValue ? (int)rec.Duration.Value.TotalMinutes : 0;
            ws.Cell(r, 7).Value = rec.Amount.HasValue ? (double)rec.Amount.Value : 0;
            ws.Cell(r, 8).Value = rec.TicketType;
            ws.Cell(r, 9).Value = rec.Status;
            r++;
        }
        ws.Columns().AdjustToContents();
    }

    // â”€â”€ Custom Range PDF â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<byte[]> GenerateCustomReportPdfAsync(int parkingId, DateTime startDate, DateTime endDate)
    {
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var start = startDate.Date;
        var end = endDate.Date.AddDays(1).AddTicks(-1);
        var records = (await _recordRepository.GetByDateRangeAsync(parkingId, start, end)).ToList();
        var subscribers = (await _subscriberRepository.GetAllAsync())
            .Count(s => s.ParkingId == parkingId && s.Status == "Active");
        var totalSpaces = parking?.TotalSpaces ?? 100;
        var spanDays = (end.Date - start).Days + 1;

        return BuildPdf(
            parking?.Name ?? "Parking",
            "Rapport Personnalisé â€” Filtré",
            $"Du {start:dd/MM/yyyy} au {end:dd/MM/yyyy}",
            records, totalSpaces, subscribers,
            col =>
            {
                AddStatsTable(col, records, totalSpaces);

                // Daily breakdown only shown for reasonably short ranges to keep the PDF readable
                if (spanDays <= 31)
                {
                    col.Item().PaddingTop(14).Text("Détail Journalier").FontSize(11).Bold();
                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });

                        static IContainer Hdr(IContainer c) =>
                            c.Background(Colors.Grey.Lighten2).Padding(5);

                        table.Header(h =>
                        {
                            h.Cell().Element(Hdr).Text("Date").Bold().FontSize(9);
                            h.Cell().Element(Hdr).Text("Entrées").Bold().FontSize(9);
                            h.Cell().Element(Hdr).Text("Sorties").Bold().FontSize(9);
                            h.Cell().Element(Hdr).Text("Tk.Perdus").Bold().FontSize(9);
                            h.Cell().Element(Hdr).Text("CA (Dh)").Bold().FontSize(9);
                        });

                        bool alt = false;
                        for (var d = start; d <= end.Date; d = d.AddDays(1))
                        {
                            var dr = records.Where(r => r.EntryTime.Date == d).ToList();
                            string bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                            table.Cell().Background(bg).Padding(4).Text(d.ToString("ddd dd/MM")).FontSize(9);
                            table.Cell().Background(bg).Padding(4).Text(dr.Count.ToString()).FontSize(9);
                            table.Cell().Background(bg).Padding(4).Text(dr.Count(r => r.ExitTime.HasValue).ToString()).FontSize(9);
                            table.Cell().Background(bg).Padding(4).Text(LostTickets(dr).ToString()).FontSize(9);
                            table.Cell().Background(bg).Padding(4).Text($"{TotalRevenue(dr):F2}").FontSize(9);
                            alt = !alt;
                        }
                    });
                }
                else
                {
                    // Long range: monthly breakdown instead of daily
                    col.Item().PaddingTop(14).Text("Résumé Mensuel").FontSize(11).Bold();
                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });

                        static IContainer Hdr(IContainer c) =>
                            c.Background(Colors.Grey.Lighten2).Padding(5);

                        table.Header(h =>
                        {
                            h.Cell().Element(Hdr).Text("Mois").Bold().FontSize(9);
                            h.Cell().Element(Hdr).Text("Entrées").Bold().FontSize(9);
                            h.Cell().Element(Hdr).Text("Sorties").Bold().FontSize(9);
                            h.Cell().Element(Hdr).Text("CA (Dh)").Bold().FontSize(9);
                        });

                        bool alt = false;
                        foreach (var g in records.GroupBy(r => new { r.EntryTime.Year, r.EntryTime.Month })
                                     .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month))
                        {
                            string bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                            var label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
                            table.Cell().Background(bg).Padding(4).Text(label).FontSize(9);
                            table.Cell().Background(bg).Padding(4).Text(g.Count().ToString()).FontSize(9);
                            table.Cell().Background(bg).Padding(4).Text(g.Count(r => r.ExitTime.HasValue).ToString()).FontSize(9);
                            table.Cell().Background(bg).Padding(4).Text($"{TotalRevenue(g.ToList()):F2}").FontSize(9);
                            alt = !alt;
                        }
                    });
                }
            }
        ).GeneratePdf();
    }



    public async Task<byte[]> GenerateDailyReportExcelAsync(int parkingId, DateTime date)
    {
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var records = (await _recordRepository.GetByDateRangeAsync(
            parkingId, date.Date, date.Date.AddDays(1).AddTicks(-1))).ToList();
        var subscribers = (await _subscriberRepository.GetAllAsync())
            .Count(s => s.ParkingId == parkingId && s.Status == "Active");

        using var wb = new XLWorkbook();
        AddKpiSheet(wb, records, parking?.Name ?? "Parking",
            $"Chiffre d'affaires du {date:dd/MM/yyyy}", parking?.TotalSpaces ?? 100, subscribers);
        AddOperatorSheet(wb, records);
        AddTransactionsSheet(wb, records);
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    // â”€â”€ Weekly Excel â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<byte[]> GenerateWeeklyReportExcelAsync(int parkingId, DateTime weekStart)
    {
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var start = weekStart.Date;
        var end = start.AddDays(7).AddTicks(-1);
        var records = (await _recordRepository.GetByDateRangeAsync(parkingId, start, end)).ToList();
        var subscribers = (await _subscriberRepository.GetAllAsync())
            .Count(s => s.ParkingId == parkingId && s.Status == "Active");

        using var wb = new XLWorkbook();
        AddKpiSheet(wb, records, parking?.Name ?? "Parking",
            $"Semaine du {start:dd/MM/yyyy} au {end:dd/MM/yyyy}", parking?.TotalSpaces ?? 100, subscribers);

        var ws2 = wb.Worksheets.Add("Détail Journalier");
        string[] h = { "Date", "Entrées", "Sorties", "Tk.Perdus", "CA (Dh)" };
        for (int i = 0; i < h.Length; i++)
        {
            ws2.Cell(1, i + 1).Value = h[i];
            ws2.Cell(1, i + 1).Style.Font.Bold = true;
            ws2.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        int r = 2;
        for (var d = start; d <= end.Date; d = d.AddDays(1))
        {
            var dr = records.Where(x => x.EntryTime.Date == d).ToList();
            ws2.Cell(r, 1).Value = d.ToString("ddd dd/MM/yyyy");
            ws2.Cell(r, 2).Value = dr.Count;
            ws2.Cell(r, 3).Value = dr.Count(x => x.ExitTime.HasValue);
            ws2.Cell(r, 4).Value = LostTickets(dr);
            ws2.Cell(r, 5).Value = (double)TotalRevenue(dr);
            r++;
        }
        ws2.Columns().AdjustToContents();

        AddOperatorSheet(wb, records);
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    // â”€â”€ Monthly Excel â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<byte[]> GenerateMonthlyReportExcelAsync(int parkingId, int year, int month)
    {
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1).AddTicks(-1);
        var records = (await _recordRepository.GetByDateRangeAsync(parkingId, start, end)).ToList();
        var subscribers = (await _subscriberRepository.GetAllAsync())
            .Count(s => s.ParkingId == parkingId && s.Status == "Active");

        using var wb = new XLWorkbook();
        AddKpiSheet(wb, records, parking?.Name ?? "Parking",
            $"{start.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))}", parking?.TotalSpaces ?? 100, subscribers);

        var ws2 = wb.Worksheets.Add("Résumé Journalier");
        string[] h = { "Date", "Entrées", "Sorties", "Tk.Perdus", "CA (Dh)" };
        for (int i = 0; i < h.Length; i++)
        {
            ws2.Cell(1, i + 1).Value = h[i];
            ws2.Cell(1, i + 1).Style.Font.Bold = true;
            ws2.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        int r = 2;
        for (var d = start; d <= end.Date; d = d.AddDays(1))
        {
            var dr = records.Where(x => x.EntryTime.Date == d).ToList();
            ws2.Cell(r, 1).Value = d.ToString("dd/MM/yyyy");
            ws2.Cell(r, 2).Value = dr.Count;
            ws2.Cell(r, 3).Value = dr.Count(x => x.ExitTime.HasValue);
            ws2.Cell(r, 4).Value = LostTickets(dr);
            ws2.Cell(r, 5).Value = (double)TotalRevenue(dr);
            r++;
        }
        ws2.Columns().AdjustToContents();

        AddOperatorSheet(wb, records);
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    // â”€â”€ Annual Excel â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<byte[]> GenerateAnnualReportExcelAsync(int parkingId, int year)
    {
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var start = new DateTime(year, 1, 1);
        var end = new DateTime(year, 12, 31, 23, 59, 59);
        var records = (await _recordRepository.GetByDateRangeAsync(parkingId, start, end)).ToList();
        var subscribers = (await _subscriberRepository.GetAllAsync())
            .Count(s => s.ParkingId == parkingId && s.Status == "Active");

        using var wb = new XLWorkbook();
        AddKpiSheet(wb, records, parking?.Name ?? "Parking",
            $"Année {year}", parking?.TotalSpaces ?? 100, subscribers);

        var ws2 = wb.Worksheets.Add("Résumé Mensuel");
        string[] h = { "Mois", "Entrées", "Sorties", "CA (Dh)" };
        for (int i = 0; i < h.Length; i++)
        {
            ws2.Cell(1, i + 1).Value = h[i];
            ws2.Cell(1, i + 1).Style.Font.Bold = true;
            ws2.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        for (int m = 1; m <= 12; m++)
        {
            var mr = records.Where(x => x.EntryTime.Month == m).ToList();
            ws2.Cell(1 + m, 1).Value = new DateTime(year, m, 1).ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
            ws2.Cell(1 + m, 2).Value = mr.Count;
            ws2.Cell(1 + m, 3).Value = mr.Count(x => x.ExitTime.HasValue);
            ws2.Cell(1 + m, 4).Value = (double)TotalRevenue(mr);
        }
        ws2.Columns().AdjustToContents();

        AddOperatorSheet(wb, records);
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    // â”€â”€ Custom Range Excel â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<byte[]> GenerateCustomReportExcelAsync(int parkingId, DateTime startDate, DateTime endDate)
    {
        var parking = await _parkingRepository.GetByIdAsync(parkingId);
        var start = startDate.Date;
        var end = endDate.Date.AddDays(1).AddTicks(-1);
        var records = (await _recordRepository.GetByDateRangeAsync(parkingId, start, end)).ToList();
        var subscribers = (await _subscriberRepository.GetAllAsync())
            .Count(s => s.ParkingId == parkingId && s.Status == "Active");

        using var wb = new XLWorkbook();
        AddKpiSheet(wb, records, parking?.Name ?? "Parking",
            $"Du {start:dd/MM/yyyy} au {end:dd/MM/yyyy}", parking?.TotalSpaces ?? 100, subscribers);

        var ws2 = wb.Worksheets.Add("Détail Journalier");
        string[] h = { "Date", "Entrées", "Sorties", "Tk.Perdus", "CA (Dh)" };
        for (int i = 0; i < h.Length; i++)
        {
            ws2.Cell(1, i + 1).Value = h[i];
            ws2.Cell(1, i + 1).Style.Font.Bold = true;
            ws2.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        int r = 2;
        for (var d = start; d <= end.Date; d = d.AddDays(1))
        {
            var dr = records.Where(x => x.EntryTime.Date == d).ToList();
            ws2.Cell(r, 1).Value = d.ToString("ddd dd/MM/yyyy");
            ws2.Cell(r, 2).Value = dr.Count;
            ws2.Cell(r, 3).Value = dr.Count(x => x.ExitTime.HasValue);
            ws2.Cell(r, 4).Value = LostTickets(dr);
            ws2.Cell(r, 5).Value = (double)TotalRevenue(dr);
            r++;
        }
        ws2.Columns().AdjustToContents();

        AddOperatorSheet(wb, records);
        AddTransactionsSheet(wb, records);
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }
}