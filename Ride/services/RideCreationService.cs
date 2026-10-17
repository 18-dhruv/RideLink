using RideService.model;

public class RideCreationService
{
    private readonly IRideRepo _riderRepo;

    public RideCreationService(IRideRepo rideRepo)
    {
        this._riderRepo=rideRepo;
    }

    public async Task<RideRes> CreateRide(RideReq req,int riderId)
    {
     var ride = new Ride(riderId,req.Pickup,req.Dropoff);
     await _riderRepo.Add(ride);
     var rideRes = new RideRes(ride.Id,null,RideStatus.Requested,ride.Pickup,ride.Dropoff);
    return rideRes;
    }
}