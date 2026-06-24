namespace ZKTeco.Parking.Application.DTOs;

public class ParkingRecordDto
{
    public int Id { get; set; }
    public string CardNo { get; set; } = string.Empty;
    public string PlateNo { get; set; } = string.Empty;
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public double? DurationMinutes { get; set; }
    public decimal? Amount { get; set; }
    public string TicketType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ParkingId { get; set; }
    public string? ParkingName { get; set; }
    public int? OperatorId { get; set; }
    public string? OperatorName { get; set; }
    public int? GateInId { get; set; }
    public string? GateInName { get; set; }
    public int? GateOutId { get; set; }
    public string? GateOutName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ParkingRecordFilterDto
{
    public int ParkingId { get; set; } = 1;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string? TicketType { get; set; }
    public string? PlateNo { get; set; }
    public string? CardNo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
