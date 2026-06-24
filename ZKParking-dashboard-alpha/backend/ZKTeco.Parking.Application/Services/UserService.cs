using ZKTeco.Parking.Application.DTOs;
using ZKTeco.Parking.Application.Interfaces;
using ZKTeco.Parking.Domain.Entities;
using ZKTeco.Parking.Domain.Interfaces;

namespace ZKTeco.Parking.Application.Services;

public class UserService : IUserService
{
    private readonly IOperatorRepository _repository;

    public UserService(IOperatorRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<OperatorDto>> GetAllAsync()
    {
        var operators = await _repository.GetAllAsync();
        return operators.Select(MapToDto);
    }

    public async Task<OperatorDto?> GetByIdAsync(int id)
    {
        var op = await _repository.GetByIdAsync(id);
        return op == null ? null : MapToDto(op);
    }

    public async Task<OperatorDto> CreateAsync(CreateOperatorDto dto)
    {
        var op = new Operator
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Role = dto.Role,
            ParkingIds = dto.ParkingIds,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(op);
        return MapToDto(created);
    }

    public async Task<OperatorDto?> UpdateAsync(int id, UpdateOperatorDto dto)
    {
        var op = await _repository.GetByIdAsync(id);
        if (op == null) return null;

        op.FullName = dto.FullName;
        op.Email = dto.Email;
        op.Phone = dto.Phone;
        op.Role = dto.Role;
        op.ParkingIds = dto.ParkingIds;
        op.IsActive = dto.IsActive;

        await _repository.UpdateAsync(op);
        return MapToDto(op);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var op = await _repository.GetByIdAsync(id);
        if (op == null) return false;
        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto)
    {
        var op = await _repository.GetByIdAsync(id);
        if (op == null) return false;

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, op.PasswordHash)) return false;

        op.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _repository.UpdateAsync(op);
        return true;
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var op = await _repository.GetByIdAsync(id);
        if (op == null) return false;

        op.IsActive = !op.IsActive;
        await _repository.UpdateAsync(op);
        return true;
    }

    private static OperatorDto MapToDto(Operator op) => new()
    {
        Id = op.Id,
        Username = op.Username,
        FullName = op.FullName,
        Email = op.Email,
        Phone = op.Phone,
        Role = op.Role,
        ParkingIds = op.ParkingIds,
        IsActive = op.IsActive,
        LastLogin = op.LastLogin,
        CreatedAt = op.CreatedAt
    };
}
