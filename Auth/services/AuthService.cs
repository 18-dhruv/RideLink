using Auth.model;
using Auth.dtos;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;
using System.Security.Cryptography;

using System.Runtime.Intrinsics.Arm;
namespace Auth.service;
public class AuthService
{
    private readonly PasswordHasher<User> PasswordHasher = new PasswordHasher<User>();
    private readonly IUserRepo UserRepo;
    private readonly IConnectionMultiplexer _redis;
    public AuthService(IUserRepo userRepo,IConnectionMultiplexer redis)
    {
        this.UserRepo = userRepo;
        this._redis =redis;
    }


    //login 
    public async Task<User> Login(LoginReq req)
    {
        var email = req.email.ToLower().Trim();
        var user = await UserRepo.GetByEmail(email);
        if (user == null||PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, req.password) == PasswordVerificationResult.Failed)
        {
            throw new UnAutorizedUser("email or password is incorrect");
        }
        return user;
    }


  //signup
    public async Task<User> Signin(RegisterReq req)
    {
        var email = req.Email.ToLower().Trim();
        var check = await UserRepo.GetByEmail(email);
        if (check != null)
        {
            throw new EntityAlreadyExist("An account with this email already exsist");
        }
    
            var user = new User(email, req.Name, req.role);
            var passwordHash = PasswordHasher.HashPassword(user,req.password);
            user.SetPassword(passwordHash);
            await UserRepo.Add(user);
            return user;
    }


   
    public async Task ResetPassword(ResetPasswordReq req)
    {
        byte[] token = Convert.FromHexString(req.Token);
        string key = "pw_reset:" + Convert.ToHexString(SHA256.HashData(token));
        IDatabase db = _redis.GetDatabase();
        var userId =await  db.StringGetDeleteAsync(key);
        if (userId.IsNull)
        {
            throw new UnAutorizedUser("invalid or expired token");
        }
        var user = await UserRepo.GetById(int.Parse(userId));
        var newHashPassword = PasswordHasher.HashPassword(user,req.NewPassword);
        user.SetPassword(newHashPassword);
        await UserRepo.Update(user);
        return;
    }

     //forgot Password 
    // for the dehashing logic first convert the string to bytes then dehash
        public async Task<string> ForgotPassword(string email)
    {
        var user = await UserRepo.GetByEmail(email);
        if (user==null)
        {
            return "";
        }
      
      byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
      string token = Convert.ToHexString(tokenBytes);
      string tokenHash = Convert.ToHexString(SHA256.HashData(tokenBytes));
      string key = "pw_reset:" + tokenHash;

      IDatabase db = _redis.GetDatabase();
      await db.StringSetAsync(key, user.Id, TimeSpan.FromSeconds(150));
      return token;
    }
}