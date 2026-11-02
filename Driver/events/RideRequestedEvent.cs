namespace Ride.model;
public record Coordinates(double Latitude, double Longitude);

public record RideRequestedEvent(int RideId, int RiderId, string Pickup, string Dropoff,
    Coordinates PickupCoordinate, Coordinates DropoffCoordinate, DateTime RequestedAt);
