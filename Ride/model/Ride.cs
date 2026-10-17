namespace RideService.model;

public class Ride : IEntity<int>
{
    public int Id { get; set; }
    public int RiderId { get; set; }
    public int? DriverId { get; set; }
    public string Pickup { get; set; }
    public string Dropoff { get; set; }
    public RideStatus Status { get; set; }


    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Postgres xmin column: EF checks it on every update, so two drivers
    // accepting the same ride at once can't both win
    public uint Version { get; set; }

    public Ride(int riderId, string pickup, string dropoff)
    {
        this.RiderId = riderId;
        this.Pickup = pickup;
        this.Dropoff = dropoff;
        this.Status = RideStatus.Requested;
        this.CreatedAt = DateTime.UtcNow;
    }
}

public enum RideStatus
{
    Requested,
    Accepted,
    InProgress,
    Completed,
    Cancelled
}
