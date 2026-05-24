using Microsoft.EntityFrameworkCore;
using SolveMachine.Models;

namespace SolveMachine.Repositories
{
    public interface IUserRepository
    {
        public Task<User?> GetUserByName(string username);
        public Task<User?> CreateUser(string username, string passwordHash, string firstName, string lastName, string email, string phone);
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

        public async Task<User?> GetUserByName(string username)
        {
            var user = await _dbContext.Users
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            _logger.LogInformation($"Successfull selected {user.Username}");
            return user;
        }

        public async Task<User?> CreateUser(string username, string passwordHash, string firstName, string lastName, string email, string phone)
        {
            try
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
                return user;
            }
            catch (DbUpdateException)
            {
                _logger.LogError($"Failed to create user {username}");
                throw;
            }
        }
    }
}