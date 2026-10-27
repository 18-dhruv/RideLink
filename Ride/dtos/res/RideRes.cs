using RideService.model;
public record class RideRes(int RideId,int? DriverId,RideStatus Status,string Pickup,string Dropoff,Coordinates PickupCoordinate, Coordinates DropoffCoordinate)
{
    public static RideRes FromRide(Ride ride)
    {
        return new RideRes(ride.Id,ride.DriverId,ride.Status,ride.Pickup,ride.Dropoff,ride.PickupCoordinates,ride.DropoffCoordinates);
    }
}
