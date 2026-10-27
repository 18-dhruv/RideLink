using RideService.model;

// Read-only access to rides, with the "who may see it" rule.
public class RideQueryService
{
    private readonly IRideRepo _rideRepo;

    public RideQueryService(IRideRepo rideRepo)
    {
        this._rideRepo=rideRepo;
    }

    // only the rider who requested the ride or the driver assigned to it may read it
    public async Task<RideRes> GetRide(int rideId, int userId)
    {
        var ride = await _rideRepo.GetById(rideId);

        if (ride == null) throw new EntityDontExist("ride not found");

        if (ride.RiderId != userId && ride.DriverId != userId)
        {
            throw new ForbiddenAction("you can only see your own rides");
        }
        return RideRes.FromRide(ride);
    }
}
