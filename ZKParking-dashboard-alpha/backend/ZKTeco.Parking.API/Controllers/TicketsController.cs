using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZKTeco.Parking.Application.DTOs;
using ZKTeco.Parking.Application.Interfaces;

namespace ZKTeco.Parking.API.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>Get paginated list of parking tickets with optional filters</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ParkingRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTickets([FromQuery] ParkingRecordFilterDto filter)
    {
        var result = await _ticketService.GetTicketsAsync(filter);
        return Ok(result);
    }

    /// <summary>Get a specific ticket by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ParkingRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicketById(int id)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id);
        if (ticket == null) return NotFound(new { message = $"Ticket with ID {id} not found" });
        return Ok(ticket);
    }

    /// <summary>Get all currently active (not exited) tickets for a parking</summary>
    [HttpGet("active/{parkingId:int}")]
    [ProducesResponseType(typeof(IEnumerable<ParkingRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveTickets(int parkingId)
    {
        var tickets = await _ticketService.GetActiveTicketsAsync(parkingId);
        return Ok(tickets);
    }
}
