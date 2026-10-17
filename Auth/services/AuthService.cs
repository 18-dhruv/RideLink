using Auth.model;
using Auth.dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public class AuthService
{
    private readonly PasswordHasher<User> PasswordHasher = new PasswordHasher<User>();
    private readonly IUserRepo UserRepo;

    public AuthService(IUserRepo userRepo)
    {
        this.UserRepo = userRepo;
    }

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
}