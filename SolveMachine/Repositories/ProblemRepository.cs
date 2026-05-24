using Microsoft.EntityFrameworkCore;
using SolveMachine.Models;

namespace SolveMachine.Repositories
{
    public interface ISelectionProblemRepository
    {
         Task<Problem?> GetProblemByName(string name, int userId);
         Task<Problem?> GetProblem(int userId, int problemId);
         Task<List<Problem>> GetAllProblems(int userId);
         Task<List<Problem>> GetFilteredProblems(int userId, DateOnly? deadLineDate, DateOnly? creationDate, ProblemPriority? priority, ProblemStatus? status);
         Task<List<Problem>> GetCandidatesForComplete();
    }

    public interface IModificationProblemRepository
    {
        Task CreateProblem(string name,
            string description,
            DateTime deadlineDate,
            int xCoord,
            int yCoord,
            ProblemPriority priority,
            ProblemStatus status,
            int userId);
        Task UpdateProblem(Problem? problem, string? name, string? description, DateTime? deadlineDate, int? xCoord, int? yCoord, ProblemPriority? priority, ProblemStatus? status);
        Task DeleteProblem(Problem? problemToDelete);
        Task SetProblemAsCompleted(Problem? problem);
    }

    public class SelectionProblemRepository : ISelectionProblemRepository
    {
        private readonly SolveMachineContext _dbContext;
        
        public SelectionProblemRepository(SolveMachineContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Problem?> GetProblem(int userId, int problemId)
        {
            var problem = await _dbContext.Problems
                .Where(e => e.UserId == userId && e.Id == problemId)
                .FirstOrDefaultAsync();
            return problem;
        }

        public async Task<Problem?> GetProblemByName(string name, int userId)
        {
            var problem = await _dbContext.Problems
                   .Where(p => p.Name == name && p.UserId == userId)
                   .FirstOrDefaultAsync();

            return problem;
        }

        public async Task<List<Problem>> GetAllProblems(int userId)
        {
            List<Problem> problems = await _dbContext.Problems
                .Where(e => e.UserId == userId)
                .ToListAsync();
            return problems;
        }

        public async Task<List<Problem>> GetFilteredProblems(int userId, DateOnly? deadLineDate, DateOnly? creationDate, ProblemPriority? priority, ProblemStatus? status)
        {
            IQueryable<Problem> query = _dbContext.Problems
                .Where(e => e.UserId == userId);

            if (deadLineDate.HasValue)
                query = query.Where(e => e.DeadlineDate == deadLineDate);

            if (creationDate.HasValue)
                query = query.Where(e => e.CreatedAt == creationDate);

            if (priority.HasValue)
                query = query.Where(e => e.Priority == priority);

            if (status.HasValue)
                query = query.Where(e => e.Status == status);

            List<Problem> queryResult = await query
                .OrderByDescending(e => e.Id)
                .ToListAsync();
            return queryResult;
        }

        public async Task<List<Problem>> GetCandidatesForComplete()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            List<Problem> expiredProblems = await _dbContext.Problems
                .Where(p => p.Status != ProblemStatus.Completed &&  today <= p.DeadlineDate)
                .ToListAsync();
            return expiredProblems;
        }
    }

    public class ModificationProblemRepository : IModificationProblemRepository
    {
        private readonly SolveMachineContext _dbContext;

        public ModificationProblemRepository(SolveMachineContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateProblem(string name,
            string description,
            DateTime deadlineDate,
            int xCoord,
            int yCoord,
            ProblemPriority priority,
            ProblemStatus status,
            int userId)
        {
            var problem = new Problem
            {
                Name = name,
                Description = description,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                DeadlineDate = DateOnly.FromDateTime(deadlineDate),
                DisplayXcoord = xCoord,
                DisplayYcoord = yCoord,
                Priority = priority,
                Status = status,
                UserId = userId
            };

            _dbContext.Problems.Add(problem);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProblem(Problem? problem, string? name, string? description, DateTime? deadlineDate, int? xCoord, int? yCoord, ProblemPriority? priority, ProblemStatus? status)
        {

            if (!string.IsNullOrEmpty(name))
                problem.Name = name;
            if (!string.IsNullOrEmpty(description))
                problem.Description = description;
            if (deadlineDate.HasValue)
                problem.DeadlineDate = DateOnly.FromDateTime(deadlineDate.Value);
            if (xCoord.HasValue)
                problem.DisplayXcoord = xCoord.Value;
            if (yCoord.HasValue)
                problem.DisplayYcoord = yCoord.Value;
            if (priority.HasValue)
                problem.Priority = priority.Value;
            if (status.HasValue)
                problem.Status = status.Value;

            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteProblem(Problem? problemToDelete)
        {
            _dbContext.Problems.Remove(problemToDelete);
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetProblemAsCompleted(Problem? problem)
        {
            problem.Status = ProblemStatus.Completed;
            problem.IsCompleted = true;
            await _dbContext.SaveChangesAsync();
        }
    }
}
