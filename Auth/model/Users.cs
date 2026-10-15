using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Auth.model;
public class User:IEntity<int>
{
 

    public int Id {get;set;}
    public string Email {get;set;}

    public string PasswordHash{get;set;}
    public string Name {get;set;}
    public Role Role{get;set;}
    public string CreatedAt
    {
        set=>DateTime.Now.ToString();
    }
    public User(string email,string name,Role role)
    {
        this.Email=email;
        this.Role=role;
        this.Name=name;
    }
    public void SetPassword(string passwordHash)
    {
        this.PasswordHash=passwordHash;
    }
}
public enum Role
{
    Driver,
    Rider
}