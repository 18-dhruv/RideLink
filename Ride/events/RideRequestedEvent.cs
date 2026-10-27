using RideService.model;

// The message Ride sends to the ride-events topic. It is a contract with other
// services (Driver), so keep it separate from RideRes and only add fields to it.
public record RideRequestedEvent(int RideId, int RiderId, string Pickup, string Dropoff,
    Coordinates PickupCoordinate, Coordinates DropoffCoordinate, DateTime RequestedAt)
{
    public static RideRequestedEvent FromRide(Ride ride)
    {
        return new RideRequestedEvent(ride.Id, ride.RiderId, ride.Pickup, ride.Dropoff,
            ride.PickupCoordinates, ride.DropoffCoordinates, ride.CreatedAt);
    }
}
