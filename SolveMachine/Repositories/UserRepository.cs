namespace SolveMachine.Repositories
{
    public interface IUserRepository
    {
        public void GetUserByName(string username);
    }

    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ILogger<UserRepository> logger)
        {
            _logger = logger;
        }

        public void GetUserByName(string username)
        {
            _logger.LogInformation($"Getting user {username}");
        }
    }
}