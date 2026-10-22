using System.ComponentModel.DataAnnotations;

public record class ForgotPasswordReq([EmailAddress]string Email);