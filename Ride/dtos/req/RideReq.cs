using RideService.model;

public record RideReq(string Pickup ,string Dropoff,Coordinates PickupCoordinate, Coordinates DropoffCoordinate);