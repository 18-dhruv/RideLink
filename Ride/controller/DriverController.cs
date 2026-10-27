using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Driver side of /rides: accept, start, complete, and cancel an accepted ride.
// Every endpoint here needs a JWT with role Driver (Auth puts it in ClaimTypes.Role).
[ApiController]
[Route("rides")]
[Authorize(Roles = "Driver")]
public class DriverController : ControllerBase
{
    private readonly RideLifecycleService _rideLifecycleService;
    private readonly ILogger<DriverController> _logger;

    public DriverController(RideLifecycleService rideLifecycleService, ILogger<DriverController> logger)
    {
        this._rideLifecycleService = rideLifecycleService;
        this._logger = logger;
    }

    private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // POST /rides/{id}/accept
    [HttpPost("{id}/accept")]
    public async Task<IActionResult> AcceptRide(int id)
    {
        _logger.LogInformation("Driver {UserId} is accepting ride {RideId}", CurrentUserId(), id);

        var ride = await _rideLifecycleService.Accept(id, CurrentUserId());

        _logger.LogInformation("Ride {RideId} accepted by driver {UserId}", id, CurrentUserId());
        return Ok(new ApiResponse<RideRes>(true, "Ride accepted successfully", ride));
    }

    // POST /rides/{id}/start
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartRide(int id)
    {
        _logger.LogInformation("Driver {UserId} is starting ride {RideId}", CurrentUserId(), id);

        var ride = await _rideLifecycleService.Start(id, CurrentUserId());

        _logger.LogInformation("Ride {RideId} started by driver {UserId}", id, CurrentUserId());
        return Ok(new ApiResponse<RideRes>(true, "Ride started successfully", ride));
    }

    // POST /rides/{id}/complete
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteRide(int id)
    {
        _logger.LogInformation("Driver {UserId} is completing ride {RideId}", CurrentUserId(), id);

        var ride = await _rideLifecycleService.Complete(id, CurrentUserId());

        _logger.LogInformation("Ride {RideId} completed by driver {UserId}", id, CurrentUserId());
        return Ok(new ApiResponse<RideRes>(true, "Ride completed successfully", ride));
    }

    // POST /rides/{id}/driver-cancel: only an accepted ride, only by its own driver
    [HttpPost("{id}/driver-cancel")]
    public async Task<IActionResult> DriverCancelRide(int id)
    {
        _logger.LogInformation("Driver {UserId} is cancelling ride {RideId}", CurrentUserId(), id);

        var ride = await _rideLifecycleService.DriverCancel(id, CurrentUserId());

        _logger.LogInformation("Ride {RideId} cancelled by driver {UserId}", id, CurrentUserId());
        return Ok(new ApiResponse<RideRes>(true, "Ride cancelled by driver", ride));
    }
}
