using Microsoft.EntityFrameworkCore;
using RideService.model;

public class RideRepo : IRideRepo
{
    private RideContext DbContext { get; set; }
    private DbSet<Ride> RideSet { get; set; }

    public RideRepo(RideContext rideContext)
    {
        this.DbContext = rideContext;
        this.RideSet = DbContext.Set<Ride>();
    }

    public async Task Add(Ride ride)
    {
        RideSet.Add(ride);
        await DbContext.SaveChangesAsync();
    }

    public async Task<Ride?> GetById(int id)
    {
        return await RideSet.FindAsync(id);
    }

    public async Task<List<Ride>> GetByStatus(RideStatus status)
    {
        return await RideSet
            .Where(e => e.Status == status)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Ride>> GetForUser(int userId)
    {
        return await RideSet
            .Where(e => e.RiderId == userId || e.DriverId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task Update(Ride ride)
    {
        try
        {
            await DbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // someone else changed this ride between our read and our write,
            // e.g. two drivers pressed accept at the same moment
            throw new InvalidStateTransition("this ride was just changed by someone else, reload it and try again");
        }
    }

    public async Task<Ride> RemoveById(int id)
    {
        var ride = await GetById(id);
        if (ride == null)
        {
            throw new EntityDontExist("ride dont exist");
        }
        RideSet.Remove(ride);
        await DbContext.SaveChangesAsync();
        return ride;
    }
}
