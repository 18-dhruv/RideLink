using RideService.model;

// Moves a ride through its lifecycle and enforces who may move it.
//
//   Requested  -> Accepted    (any driver)
//   Requested  -> Cancelled   (the rider)
//   Accepted   -> InProgress  (the ride's driver)
//   Accepted   -> Cancelled   (the rider or the ride's driver)
//   InProgress -> Completed   (the ride's driver)
//
// Anything else throws InvalidStateTransition (409); the wrong user throws ForbiddenAction (403).
// It only changes the Ride object in memory; the caller saves it.
public class RideLifecycleService(IRideRepo rideRepo)
{
    private IRideRepo _rideRepo = rideRepo;

    public async Task<RideRes> Accept(int rideId, int driverId)
    {
        var ride = await _rideRepo.GetById(rideId);

        if (ride == null) throw new EntityDontExist("ride not found");
        
         if (ride.Status != RideStatus.Requested)
        {
            throw new InvalidStateTransition("cannot start a ride that is in "+ride.Status);
        }
        ride.DriverId=driverId;
        ride.Status=RideStatus.Accepted;
        ride.AcceptedAt=DateTime.UtcNow;
        await _rideRepo.Update(ride);
        var rideRes= RideRes.FromRide(ride);
        return rideRes;
    }

    public async Task<RideRes> Start(int rideId, int driverId)
    {
        var ride = await _rideRepo.GetById(rideId);

        if (ride == null) throw new EntityDontExist("ride not found");

        if (ride.DriverId != driverId)
        {
            throw new ForbiddenAction("u cant start some other drivers ride");
        }
         if (ride.Status != RideStatus.Accepted)
        {
            throw new InvalidStateTransition("cannot start a ride that is in "+ride.Status);
        }
        ride.Status=RideStatus.InProgress;
        ride.StartedAt=DateTime.UtcNow;
        await _rideRepo.Update(ride);
        var rideRes= RideRes.FromRide(ride);
        return rideRes;
    }

    public async Task<RideRes> Complete(int rideId, int driverId)
    {
        var ride = await _rideRepo.GetById(rideId);

        if (ride == null) throw new EntityDontExist("ride not found");

        if (ride.DriverId != driverId)
        {
            throw new ForbiddenAction("u cant complete some other drivers ride");
        }
        if (ride.Status != RideStatus.InProgress)
        {
            throw new InvalidStateTransition("cannot complete a ride that is in "+ride.Status);
        }
        ride.Status=RideStatus.Completed;
        ride.CompletedAt=DateTime.UtcNow;
        await _rideRepo.Update(ride);
        var rideRes= RideRes.FromRide(ride);
        return rideRes;
    }

    public async Task<RideRes> Cancel(int rideId, int userId)
    {
        var ride = await _rideRepo.GetById(rideId);

        if (ride == null) throw new EntityDontExist("ride not found");

        if (ride.RiderId != userId)
        {
            throw new ForbiddenAction("you cant cancel other user ride");
        }
        if (ride.Status != RideStatus.Requested && ride.Status != RideStatus.Accepted)
        {
            throw new InvalidStateTransition("cannot cancel a ride that is in "+ride.Status);
        }
        ride.Status=RideStatus.Cancelled;
        ride.CancelledAt=DateTime.UtcNow;
        await _rideRepo.Update(ride);
        var rideRes= RideRes.FromRide(ride);
        return rideRes;
    }

    // a driver can only cancel a ride they accepted and haven't started yet
    public async Task<RideRes> DriverCancel(int rideId, int driverId)
    {
        var ride = await _rideRepo.GetById(rideId);

        if (ride == null) throw new EntityDontExist("ride not found");

        if (ride.DriverId != driverId)
        {
            throw new ForbiddenAction("you cant cancel some other drivers ride");
        }
        if (ride.Status != RideStatus.Accepted)
        {
            throw new InvalidStateTransition("cannot cancel a ride that is in "+ride.Status);
        }
        ride.Status=RideStatus.Cancelled;
        ride.CancelledAt=DateTime.UtcNow;
        await _rideRepo.Update(ride);
        var rideRes= RideRes.FromRide(ride);
        return rideRes;
    }
}
