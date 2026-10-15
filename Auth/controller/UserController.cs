using Auth.dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

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
     return Ok(token.CreateToken(user));
    }
    [HttpPost("signin")]
    public async Task<IActionResult> Register([FromBody]RegisterReq req)
    {
        var user =await auth.Signin(req);
        return Ok(token.CreateToken(user));
    }

}