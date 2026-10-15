using Auth.model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

public class RefreshTokenRepo : IRepo<int, RefreshToken>
{
    private AuthContext DbContext{get;set;}
    private DbSet<RefreshToken> DbSet{get;set;}   
    public RefreshTokenRepo(AuthContext authContext)
    {
        this.DbContext=authContext;
        this.DbSet=DbContext.Set<RefreshToken>();
    }
     public async Task Add(RefreshToken entity)
    {
       DbSet.Add(entity);
       await DbContext.SaveChangesAsync();
    }
    

    public async Task<RefreshToken> GetById(int key)
    {
      var refreshToken= await  DbSet.FindAsync(key);
      return refreshToken;
    }

    public async Task<RefreshToken> RemoveBYId(int key)
    {
       var refreshToken=await  GetById(key);
        if (refreshToken == null)
        {
            throw new EntityDontExist("Refresh Tocken Dont exist");
        }
       DbSet.Remove(refreshToken);
       await DbContext.SaveChangesAsync();
       return refreshToken;
    }
}