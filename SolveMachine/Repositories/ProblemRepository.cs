using Microsoft.EntityFrameworkCore;
using SolveMachine.Models;

namespace SolveMachine.Repositories
{
    public interface ISelectionProblemRepository
    {
         Task<ProblemResult> GetProblemByName(string name, int userId);
         Task<ProblemResult> GetAllProblems(int userId);
         Task<ProblemResult> GetFilteredProblems(int userId, DateOnly? deadLineDate, DateOnly? creationDate, ProblemPriority? priority, ProblemStatus? status);
         Task<ProblemResult> GetExpiredProblemsForAllUsers();
    }

    public interface IModificationProblemRepository
    {
        Task<ProblemResult> CreateProblem(string name,
            string description,
            DateTime deadlineDate,
            int xCoord,
            int yCoord,
            ProblemPriority priority,
            ProblemStatus status,
            int userId);
        Task<ProblemResult> UpdateProblem(int userId, int problemId, string? name, string? description, DateTime? deadlineDate, int? xCoord, int? yCoord, ProblemPriority? priority, ProblemStatus? status);
        Task<ProblemResult> DeleteProblem(int userId, int problemId);
        Task<ProblemResult> SetProblemAsCompleted(int problemId);
    }

    public class SelectionProblemRepository : ISelectionProblemRepository
    {
        private readonly SolveMachineContext _dbContext;
        
        public SelectionProblemRepository(SolveMachineContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProblemResult> GetProblemByName(string name, int userId)
        {
            var problem = await _dbContext.Problems
                   .Where(p => p.Name == name && p.UserId == userId)
                   .Select(p => new Problem
                   {
                       Id = p.Id,
                       Name = p.Name,
                       Description = p.Description,
                       CreatedAt = p.CreatedAt,
                       DeadlineDate = p.DeadlineDate,
                       DisplayXcoord = p.DisplayXcoord,
                       DisplayYcoord = p.DisplayYcoord,
                       Priority = p.Priority,
                       Status = p.Status
                   })
                   .FirstOrDefaultAsync();

            if (problem == null)
                return new ProblemResult { IsSuccess = false, ErrorMessage = $"Problem by {name} does not exists"};

            return new ProblemResult { IsSuccess = true, Problem = problem };
        }

        public async Task<ProblemResult> GetAllProblems(int userId)
        {
            List<Problem> problems = await _dbContext.Problems
                .Where(e => e.UserId == userId)
                .Select(p => new Problem
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    DeadlineDate = p.DeadlineDate,
                    DisplayXcoord = p.DisplayXcoord,
                    DisplayYcoord = p.DisplayYcoord,
                    Priority = p.Priority,
                    Status = p.Status
                })
                .ToListAsync();
            return new ProblemResult { IsSuccess = true, Problems = problems };
        }

        public async Task<ProblemResult> GetFilteredProblems(int userId, DateOnly? deadLineDate, DateOnly? creationDate, ProblemPriority? priority, ProblemStatus? status)
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
                .Select(p => new Problem
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    DeadlineDate = p.DeadlineDate,
                    DisplayXcoord = p.DisplayXcoord,
                    DisplayYcoord = p.DisplayYcoord,
                    Priority = p.Priority,
                    Status = p.Status
                })
                .OrderByDescending(e => e.Id)
                .ToListAsync();
            return new ProblemResult { IsSuccess = true, Problems = queryResult };
        }

        public async Task<ProblemResult> GetExpiredProblemsForAllUsers()
        {
            List<Problem> expiredProblems = await _dbContext.Problems
                .Where(p => p.Status != ProblemStatus.Completed && p.CreatedAt <= p.DeadlineDate && !p.IsCompleted)
                .ToListAsync();
            return new ProblemResult { IsSuccess = true, Problems = expiredProblems };
        }
    }

    public class ModificationProblemRepository : IModificationProblemRepository
    {
        private readonly SolveMachineContext _dbContext;

        public ModificationProblemRepository(SolveMachineContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProblemResult> CreateProblem(string name,
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
            return new ProblemResult { IsSuccess = true, Problem = problem };
        }

        public async Task<ProblemResult> UpdateProblem(int userId, int problemId, string? name, string? description, DateTime? deadlineDate, int? xCoord, int? yCoord, ProblemPriority? priority, ProblemStatus? status)
        {
            var problem = await _dbContext.Problems
                .Where(p => p.Id == problemId && p.UserId == userId)
                .FirstOrDefaultAsync();

            if (problem == null)
                return new ProblemResult { IsSuccess = false, ErrorMessage = $"Problem with id {problemId} does not exists" };

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
            return new ProblemResult { IsSuccess = true, Problem = problem };
        }
        public async Task<ProblemResult> DeleteProblem(int userId, int problemId)
        {
            var problem = await _dbContext.Problems
                .Where(e => e.Id == problemId && e.UserId == userId)
                .FirstOrDefaultAsync();

            if (problem == null)
                return new ProblemResult { IsSuccess = false, ErrorMessage = $"Problem with id {problemId} does not exists" };

            _dbContext.Problems.Remove(problem);
            await _dbContext.SaveChangesAsync();
            return new ProblemResult { IsSuccess = true };
        }

        public async Task<ProblemResult> SetProblemAsCompleted(int problemId)
        {
            var problem = await _dbContext.Problems
                .Where(e => e.Id == problemId)
                .FirstOrDefaultAsync();
            if (problem == null)
                return new ProblemResult { IsSuccess = false, ErrorMessage = $"Problem with id {problemId} does not exists" };

            problem.Status = ProblemStatus.Completed;
            problem.IsCompleted = true;
            await _dbContext.SaveChangesAsync();
            return new ProblemResult { IsSuccess = true, Problem = problem };
        }
    }
}
