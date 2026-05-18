using Microsoft.EntityFrameworkCore;
using SolveMachine.Models;

namespace SolveMachine.Repositories
{
    public interface IUserRepository
    {
        public Task<UserResult> GetUserByName(string username);
        public Task<UserResult> CreateUser(string username, string passwordHash, string firstName, string lastName, string email, string phone);
    }

    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly SolveMachineContext _dbContext;

        public UserRepository(ILogger<UserRepository> logger, SolveMachineContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<UserResult> GetUserByName(string username)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) 
                return new UserResult { IsSuccess = false, ErrorMessage = $"User by {username} does not exists"};

            _logger.LogInformation($"Successfull selected {user.Username}");
            return new UserResult { IsSuccess = true, SelectedUser = user };
        }

        public async Task<UserResult> CreateUser(string username, string passwordHash, string firstName, string lastName, string email, string phone)
        {
             var user = new User
             {
                 Username = username,
                 PasswordHash = passwordHash,
                 FirstName = firstName,
                 LastName = lastName,
                 Email = email,
                 Phone = phone,
                 CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                 IsActive = true
              };
              _dbContext.Users.Add(user);
              await _dbContext.SaveChangesAsync();
              return new UserResult { IsSuccess = true, SelectedUser = user };
        }
    }
}