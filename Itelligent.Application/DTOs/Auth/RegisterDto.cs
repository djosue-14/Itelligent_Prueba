using System.ComponentModel.DataAnnotations;

namespace Itelligent.Application.DTOs.Auth;

public class RegisterDto
{
    [Required, MinLength(3)]
    public string Username { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
