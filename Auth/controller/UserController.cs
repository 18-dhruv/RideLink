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
    public UserController(AuthService auth,TokenService _token)
    {
        this.auth=auth;
        this.token=_token;
    }
 
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody]LoginReq req)
    {
     var user = await auth.Login(req);
     return Ok(new ApiResponse<string>(true,"Login successful",token.CreateToken(user)));
    }


    [HttpPost("signin")]
    public async Task<IActionResult> Register([FromBody]RegisterReq req)
    {
        var user =await auth.Signin(req);
        return Ok(new ApiResponse<string>(true,"User registered successfully",token.CreateToken(user)));
    }
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordReq req)
    {
       await auth.ResetPassword(req);
       return Ok(new ApiResponse<string>(true,"password reset sucessfull",null));
    } 

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordReq req)
    {
        var email=req.Email.ToLower().Trim();
        var token =await auth.ForgotPassword(email);
        if (token.Length == 0)
        {
            return Ok(new ApiResponse<string>(true,"Forgot password mail sent on the requested mail",token));
        }
        return Ok(new ApiResponse<string>(true,"Forgot password mail sent on the requested mail",token))    ;
    }

}