using System.ComponentModel.DataAnnotations;

namespace SolveMachine.Models
{
    public record LoginDto(string Username, string Password);

    public record RegisterDto(string Username,
        string Password,
        string RepeatPassword,
        string FirstName,
        string LastName,
        [EmailAddress(ErrorMessage = "Invalid email format")] string Email,
        [Phone(ErrorMessage = "Invalid phone format")] string Phone);

    public record ProfileDto(string Username, string FirstName, string LastName, string Email, string Phone);

    public record ProblemCreationDto(
        string Name,
        string Description,
        DateTime DeadLineDate, 
        int XCoord,
        int YCoord,
        ProblemPriority Priority,
        ProblemStatus Status
    );

    public record ProblemFilteringDto(string name, DateOnly? DeadLineDate, ProblemPriority? Priority, ProblemStatus? Status, DateOnly? CreationDate);
}