using System.ComponentModel.DataAnnotations;

namespace RideService.model;

public class Ride : IEntity<int>
{
    public int Id { get; set; }
    public int RiderId { get; set; }
    public int? DriverId { get; set; }
    public string Pickup { get; set; }
    public Coordinates  PickupCoordinates{get;set;}
    public string Dropoff { get; set; }
    public Coordinates DropoffCoordinates { get; set; }
    public RideStatus Status { get; set; }


    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Postgres xmin column: EF checks it on every update, so two drivers
    // accepting the same ride at once can't both win
    public uint Version { get; set; }

    public void SetPickupCoordinate(Coordinates coordinates)
    {
        this.PickupCoordinates=coordinates;
    }

     public void SetDropoffCoordinate(Coordinates coordinates)
    {
       this.DropoffCoordinates=coordinates;
    }
    public Ride(int riderId, string pickup, string dropoff)
    {
        this.RiderId = riderId;
        this.Pickup = pickup;
        this.Dropoff = dropoff;
        this.Status = RideStatus.Requested;
        this.CreatedAt = DateTime.UtcNow;
    }
}
public record Coordinates([Range(-90, 90)] double Latitude, [Range(-180, 180)] double Longitude);
public enum RideStatus
{
    Requested,
    Accepted,
    InProgress,
    Completed,
    Cancelled
}
