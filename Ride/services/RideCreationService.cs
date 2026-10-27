using RideService.model;

public class RideCreationService
{
    private readonly IRideRepo _riderRepo;
    private readonly RidePublish _publisher;

    public RideCreationService(IRideRepo rideRepo, RidePublish publisher)
    {
        this._riderRepo=rideRepo;
        this._publisher=publisher;
    }

    public async Task<RideRes> CreateRide(RideReq req,int riderId)
    {
     var ride = new Ride(riderId,req.Pickup,req.Dropoff);
     ride.SetDropoffCoordinate(req.DropoffCoordinate);
     ride.SetPickupCoordinate(req.PickupCoordinate);
     await _riderRepo.Add(ride);
     // save first so ride.Id is the real id, then tell other services
     await _publisher.PublishRideAsync(RideRequestedEvent.FromRide(ride));
     var rideRes = RideRes.FromRide(ride);
     return rideRes;
    }
}