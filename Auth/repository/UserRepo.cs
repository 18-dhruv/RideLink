using Auth.model;
using Microsoft.EntityFrameworkCore;

public class UserRepo:IUserRepo
{
    private AuthContext DbContext{get;set;}
    private DbSet<User> UserSet{get;set;}
    
    public UserRepo(AuthContext authContext)
    {
        this.DbContext=authContext;
        this.UserSet=DbContext.Set<User>();
    }
    
    public async Task<User> GetByEmail(string email)
    {
        email= email.ToLower().Trim();
        var user = await UserSet.FirstOrDefaultAsync(e=>e.Email==email);
        return user;
    }


    public async Task Add(User user)
    {
      UserSet.Add(user);
      await DbContext.SaveChangesAsync();
    }

    public async Task<User> GetById(int id)
    {
        var user = await UserSet.FindAsync(id);
        return user;
    }
    async Task<User> IRepo<int, User>.GetById(int id)
    {
        return await GetById(id);
    }
    public async Task<User> RemoveBYId(int id)
    {
        var user = await GetById(id);
        if (user == null)
        {
         throw new EntityDontExist("user dont Exist");
        }
        UserSet.Remove(user);
        await DbContext.SaveChangesAsync();
        return user;
    }
}