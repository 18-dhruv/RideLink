using Auth.model;
using Auth.dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

public class AuthService
{
    private readonly PasswordHasher<User> PasswordHasher = new PasswordHasher<User>();
    private readonly IUserRepo UserRepo;

    public AuthService(IUserRepo userRepo)
    {
        this.UserRepo = userRepo;
    }

    // public async Task<User> Login(string email, str=ring password)
    public async Task<User> Login(LoginReq req)
    {
        var user = await UserRepo.GetByEmail(req.email);
        if (user == null||PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, req.password) == PasswordVerificationResult.Failed)
        {
            throw new UnAutorizedUser("email or password is incorrect");
        }
        return user;
    }

    public async Task<User> Signin(RegisterReq req)
    {
     var user= await UserRepo.GetByEmail(req.Email);
        if (user != null)
        {
           throw new UnAutorizedUser("email already exist");
        }
            user = new User(req.Email, req.Name, req.role);
            var passwordHash = PasswordHasher.HashPassword(user,req.password);
            user.SetPassword(passwordHash);
            await UserRepo.Add(user);
            return user;
    }
}