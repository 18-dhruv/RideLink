using Auth.model;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
namespace Auth.service;

public class TokenService
{
   private readonly IConfiguration configuration;
   
   public TokenService(IConfiguration _configuration)
    {
        this.configuration=_configuration;
    }

    public string CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
        var descriptor = new SecurityTokenDescriptor{
            Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Role,user.Role.ToString()),
                ]
            ),
            Audience = configuration["Jwt:Audience"],
            Issuer   = configuration["Jwt:Issuer"],
            Expires  = DateTime.UtcNow.AddMinutes(Convert.ToInt64(configuration["Jwt:Expiry"])),
            SigningCredentials = new SigningCredentials(key,SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    } 

    public string CreateRefreshToken()
    {
        throw new NotImplementedException();
    }

   
}