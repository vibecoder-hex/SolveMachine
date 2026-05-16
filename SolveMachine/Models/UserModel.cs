using System;
using System.Collections.Generic;

namespace SolveMachine.Models;

public enum UserRole { Admin, Manager, User }

public partial class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool? IsActive { get; set; }
    public DateOnly? CreatedAt { get; set; }
    public UserRole Role { get; set; }
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
