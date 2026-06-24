using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZKTeco.Parking.Application.DTOs;
using ZKTeco.Parking.Application.Interfaces;

namespace ZKTeco.Parking.API.Controllers;

[ApiController]
[Route("api/subscribers")]
[Authorize]
public class SubscribersController : ControllerBase
{
    private readonly ISubscriberService _subscriberService;

    public SubscribersController(ISubscriberService subscriberService)
    {
        _subscriberService = subscriberService;
    }

    /// <summary>Get all subscribers for a parking</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SubscriberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int parkingId = 1)
    {
        var subscribers = await _subscriberService.GetAllAsync(parkingId);
        return Ok(subscribers);
    }

    /// <summary>Get a subscriber by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var subscriber = await _subscriberService.GetByIdAsync(id);
        if (subscriber == null) return NotFound(new { message = $"Subscriber with ID {id} not found" });
        return Ok(subscriber);
    }

    /// <summary>Create a new subscriber</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubscriberDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSubscriberDto dto)
    {
        var subscriber = await _subscriberService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = subscriber.Id }, subscriber);
    }

    /// <summary>Update an existing subscriber</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubscriberDto dto)
    {
        var subscriber = await _subscriberService.UpdateAsync(id, dto);
        if (subscriber == null) return NotFound(new { message = $"Subscriber with ID {id} not found" });
        return Ok(subscriber);
    }

    /// <summary>Delete a subscriber</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _subscriberService.DeleteAsync(id);
        if (!result) return NotFound(new { message = $"Subscriber with ID {id} not found" });
        return NoContent();
    }

    /// <summary>Renew a subscriber's subscription</summary>
    [HttpPut("{id:int}/renew")]
    [ProducesResponseType(typeof(SubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Renew(int id, [FromBody] RenewSubscriberDto dto)
    {
        var subscriber = await _subscriberService.RenewAsync(id, dto);
        if (subscriber == null) return NotFound(new { message = $"Subscriber with ID {id} not found" });
        return Ok(subscriber);
    }

    /// <summary>Get subscribers expiring within the specified number of days</summary>
    [HttpGet("expiring/{days:int}")]
    [ProducesResponseType(typeof(IEnumerable<SubscriberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiring(int days, [FromQuery] int parkingId = 1)
    {
        var subscribers = await _subscriberService.GetExpiringAsync(parkingId, days);
        return Ok(subscribers);
    }
}
