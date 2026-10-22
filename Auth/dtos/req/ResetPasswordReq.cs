using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

public record class ResetPasswordReq(string Token,string NewPassword);