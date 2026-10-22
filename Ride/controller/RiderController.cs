using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Rider side of /rides: request a ride, read it, cancel it.
[ApiController]
[Route("rides")]
[Authorize]
public class RiderController : ControllerBase
{
    private readonly RideCreationService _rideCreationService;
    private readonly RideLifecycleService _rideLifecycleService;
    private readonly RideQueryService _rideQueryService;

    public RiderController(RideCreationService rideCreationService,RideLifecycleService rideLifecycleService,RideQueryService rideQueryService)
    {
        this._rideCreationService=rideCreationService;
        this._rideLifecycleService=rideLifecycleService;
        this._rideQueryService=rideQueryService;
    }

    private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // POST /rides
    [Authorize(Roles ="Rider")]
    [HttpPost]
    public async Task<IActionResult> CreateRide([FromBody]RideReq req)
    {
        var ride = await _rideCreationService.CreateRide(req,CurrentUserId());
        return CreatedAtAction(nameof(GetRide), new { id = ride.RideId }, new ApiResponse<RideRes>(true,"Ride created successfully",ride));
    }

    // GET /rides/{id}: the rider or the assigned driver (owner check is in RideQueryService)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRide(int id)
    {
        var ride = await _rideQueryService.GetRide(id,CurrentUserId());
        return Ok(new ApiResponse<RideRes>(true,"Ride fetched successfully",ride));
    }

    // POST /rides/{id}/cancel
    [Authorize(Roles ="Rider")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelRide(int id)
    {
        var ride = await _rideLifecycleService.Cancel(id,CurrentUserId());
        return Ok(new ApiResponse<RideRes>(true,"Ride cancelled successfully",ride));
    }
}
