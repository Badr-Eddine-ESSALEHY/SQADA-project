using ZKTeco.Parking.Application.DTOs;

namespace ZKTeco.Parking.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<OperatorDto>> GetAllAsync();
    Task<OperatorDto?> GetByIdAsync(int id);
    Task<OperatorDto> CreateAsync(CreateOperatorDto dto);
    Task<OperatorDto?> UpdateAsync(int id, UpdateOperatorDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto);
    Task<bool> ToggleActiveAsync(int id);
}
