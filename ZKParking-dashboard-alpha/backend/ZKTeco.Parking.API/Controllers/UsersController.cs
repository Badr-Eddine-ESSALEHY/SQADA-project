using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZKTeco.Parking.Application.DTOs;
using ZKTeco.Parking.Application.Interfaces;

namespace ZKTeco.Parking.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Get all operators/users</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Supervisor")]
    [ProducesResponseType(typeof(IEnumerable<OperatorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    /// <summary>Get a user by ID</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Supervisor")]
    [ProducesResponseType(typeof(OperatorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound(new { message = $"User with ID {id} not found" });
        return Ok(user);
    }

    /// <summary>Create a new operator/user</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OperatorDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateOperatorDto dto)
    {
        var user = await _userService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>Update an existing operator/user</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OperatorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOperatorDto dto)
    {
        var user = await _userService.UpdateAsync(id, dto);
        if (user == null) return NotFound(new { message = $"User with ID {id} not found" });
        return Ok(user);
    }

    /// <summary>Delete an operator/user</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);
        if (!result) return NotFound(new { message = $"User with ID {id} not found" });
        return NoContent();
    }

    /// <summary>Change password for an operator/user</summary>
    [HttpPut("{id:int}/change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
    {
        var result = await _userService.ChangePasswordAsync(id, dto);
        if (!result) return BadRequest(new { message = "Invalid current password or user not found" });
        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>Toggle active status for an operator/user</summary>
    [HttpPut("{id:int}/toggle-active")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _userService.ToggleActiveAsync(id);
        if (!result) return NotFound(new { message = $"User with ID {id} not found" });
        return Ok(new { message = "User status toggled successfully" });
    }
}
