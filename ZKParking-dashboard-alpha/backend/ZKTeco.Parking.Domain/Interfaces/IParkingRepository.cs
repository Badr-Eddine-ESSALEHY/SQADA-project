using ParkingEntity = ZKTeco.Parking.Domain.Entities.Parking;

namespace ZKTeco.Parking.Domain.Interfaces;

public interface IParkingRepository : IRepository<ParkingEntity>
{
    Task<IEnumerable<ParkingEntity>> GetActiveParkingsAsync();
    Task<ParkingEntity?> GetByCodeAsync(string code);
}