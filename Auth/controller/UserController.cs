using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Auth.dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Auth.service;
namespace Auth.Controller;
[ApiController]
[Route("/auth")]
public class UserController : ControllerBase
{
    private readonly AuthService auth;
    private readonly TokenService token;

    private readonly ILogger<UserController>logger;
    public UserController(AuthService _auth,TokenService _token,ILogger<UserController> _logger)
    {
        this.auth=_auth;
        this.token=_token;
        this.logger=_logger;
    }
 
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody]LoginReq req)
    {
     logger.LogInformation("login attempt for {Email}",req.email);

     var user = await auth.Login(req);

     logger.LogInformation("User {UserId} successfully logged in",user.Id);

     return Ok(new ApiResponse<string>(true,"Login successfully",token.CreateToken(user)));
    }


    [HttpPost("signin")]
    public async Task<IActionResult> Register([FromBody]RegisterReq req)
    {

        logger.LogInformation("Register attempt for {Email} as {Role}",req.Email,req.role);

        var user =await auth.Signin(req);

        logger.LogInformation("User {UserId} registered successfully",user.Id);
        return Ok(new ApiResponse<string>(true,"User registered successfully",token.CreateToken(user)));
    }
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordReq req)
    {
       logger.LogInformation("Password reset attempt");

       await auth.ResetPassword(req);

       logger.LogInformation("Password reset successfully");
       return Ok(new ApiResponse<string>(true,"password reset sucessfull",null));
    } 

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordReq req)
    {
        var email=req.Email.ToLower().Trim();
        logger.LogInformation("Forgot password requested for {Email}",email);
        var token =await auth.ForgotPassword(email);
        if (token.Length == 0)
        {
            return Ok(new ApiResponse<string>(true,"Forgot password mail sent on the requested mail",token));
        }
        return Ok(new ApiResponse<string>(true,"Forgot password mail sent on the requested mail",token))    ;
    }

}