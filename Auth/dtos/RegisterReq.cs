using System.ComponentModel.DataAnnotations;
using Auth.model;
namespace Auth.dtos;
public record class RegisterReq(string Name,[EnumDataType(typeof(Role))] Role role,string Email,string password);


//    public int UserId {get;set;}
//     public string Email {get;set;}
//     public string PasswordHash{get;set;}
//     public string Name {get;set;}
//     public Role Role{get;set;}