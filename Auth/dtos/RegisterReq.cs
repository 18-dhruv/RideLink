using Auth.model;
namespace Auth.dtos;
public record class RegisterReq(string Name,Role role,string Email,string password);


//    public int UserId {get;set;}
//     public string Email {get;set;}
//     public string PasswordHash{get;set;}
//     public string Name {get;set;}
//     public Role Role{get;set;}