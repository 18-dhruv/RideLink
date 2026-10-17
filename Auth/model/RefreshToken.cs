using System.Security.Cryptography.X509Certificates;

namespace Auth.model;
public class RefreshToken:IEntity<int>
{
    public int Id{get;set;}
    public int UserId{get;set;}
    public string TokenHash{get;set;}
    public DateTime ExpiresAt{get;set;}
    public DateTime? RevokeAt{get;set;}
    public int ReplacedById{get;set;}
    public RefreshToken(int userId,string tokenHash,int replacedById)
    {
        this.UserId=userId;
        this.TokenHash=tokenHash;
        this.ReplacedById=replacedById;
    }
}