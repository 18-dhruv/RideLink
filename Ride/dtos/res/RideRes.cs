using RideService.model;
public record class RideRes(int RideId,int? DriverId,RideStatus Status,string Pickup,string Dropoff);