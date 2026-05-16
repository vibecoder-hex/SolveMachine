using System;
using System.Collections.Generic;

namespace SolveMachine.Models;

public enum TaskStatus { Completed, InProcess, DidntStarted }
public enum TaskPriority { Low, Medium, High }

public partial class Task
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly CreatedAt { get; set; }
    public DateOnly DeadlineDate { get; set; }
    public bool IsCompleted { get; set; }
    public int DisplayXcoord { get; set; }
    public int DisplayYcoord { get; set; }
    public int UserId { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public virtual User User { get; set; } = null!;
}
