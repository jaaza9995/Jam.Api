using System.ComponentModel.DataAnnotations;

namespace Jam.Api.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "Please enter a valid username.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a valid password.")]
    public string Password { get; set; } = string.Empty;
}