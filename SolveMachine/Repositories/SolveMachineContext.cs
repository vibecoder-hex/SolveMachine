using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SolveMachine.Models;

namespace SolveMachine.Repositories;

public partial class SolveMachineContext : DbContext
{
    public SolveMachineContext()
    {
    }

    public SolveMachineContext(DbContextOptions<SolveMachineContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=solveMachine;Username=vibecoderhex;Password=azsxdcQ1!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("task_priority", new[] { "high", "medium", "low" })
            .HasPostgresEnum("task_status", new[] { "in_process", "completed", "did_not_start" })
            .HasPostgresEnum("user_role", new[] { "admin", "user", "manager" });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Task_pkey");

            entity.ToTable("Task");

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

            entity.HasOne(d => d.User).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Task_user_id_fkey");
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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
