using Microsoft.EntityFrameworkCore;
using SolveMachine.Models;

namespace SolveMachine.Repositories;

public partial class SolveMachineContext : DbContext
{
    public SolveMachineContext(DbContextOptions<SolveMachineContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Problem> Problems { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Problem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Task_pkey");

            entity.ToTable("Problem");

            entity.HasIndex(e => e.UserId, "idx_problem_user_id");

            entity.HasIndex(e => e.UserId, "idx_task_userid");

            entity.HasIndex(e => new { e.Id, e.Name }, "task_uniques").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.DeadlineDate).HasColumnName("deadline_date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisplayXcoord).HasColumnName("display_xcoord");
            entity.Property(e => e.DisplayYcoord).HasColumnName("display_ycoord");
            entity.Property(e => e.IsCompleted).HasColumnName("is_completed");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Problems)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Task_user_id_fkey");
            entity.Property(e => e.Status)
                .HasColumnType("problem_status")
                .HasColumnName("status");
            entity.Property(e => e.Priority)
                .HasColumnName("priority")
                .HasColumnType("problem_priority");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => new { e.Id, e.Username, e.Phone, e.Email }, "user_uniques").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(40)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(40)
                .HasColumnName("first_name");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(40)
                .HasColumnName("last_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(40)
                .HasColumnName("phone");
            entity.Property(e => e.Username)
                .HasMaxLength(40)
                .HasColumnName("username");
            entity.Property(e => e.Role)
                .HasColumnType("user_role")
                .HasColumnName("role");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .HasColumnName("password_hash");
        });
    }
}
