using RideService.model;

public interface IRideRepo : IRepo<int, Ride>
{
    Task<List<Ride>> GetByStatus(RideStatus status);
    Task<List<Ride>> GetForUser(int userId);
    Task Update(Ride ride);
}
