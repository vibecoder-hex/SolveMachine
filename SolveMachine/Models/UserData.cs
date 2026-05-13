using System.ComponentModel.DataAnnotations;

namespace SolveMachine.Models
{
    public record LoginDto(string Username, string Password);

    public record RegisterDto(string Username,
        string Password,
        string RepeatPassword,
        [EmailAddress(ErrorMessage = "Invalid email format")] string Email,
        [Phone(ErrorMessage = "Invalid phone format")] string Phone);
}