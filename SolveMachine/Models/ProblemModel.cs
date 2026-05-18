namespace SolveMachine.Models;

public enum ProblemPriority { High, Medium, Low}
public enum ProblemStatus { Completed, NotStarted, InProccess }
public partial class Problem
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
    public ProblemPriority Priority {  get; set; }
    public ProblemStatus Status { get; set; }
    public virtual User User { get; set; } = null!;
}
