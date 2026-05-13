using SolveMachine.Repositories;

namespace SolveMachine.Services
{
    public interface ILoginService
    {
        public void Login(string username, string password);
    }

    public interface IRegistrationService
    {
        public void Register(string username, string password, string repeatPassword, string email, string phone);
    }

    public interface ITokenService
    {
        string GetJsonWebTokenString(string username);
    }

    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;

        public LoginService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public void Login(string username, string password)
        {
            _userRepository.GetUserByName(username);
        }
    }

    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger;

        public TokenService(ILogger<TokenService> logger)
        {
            _logger = logger;
        }

        public string GetJsonWebTokenString(string username)
        {
            return "";
        }
    }

    public class RegistrationService : IRegistrationService
    {
        private readonly IUserRepository _userRepository;

        public RegistrationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public void Register(string username, string password, string repeatPassword, string email, string phone)
        {
            _userRepository.GetUserByName(username);
        }
    }
}