namespace SolveMachine.Models;

public enum UserRole { Admin, Manager, User }

public partial class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateOnly CreatedAt { get; set; }
    public UserRole Role { get; set; }
    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
}
