using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SolveMachine.Models;
using SolveMachine.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SolveMachine.Services
{
    public interface ILoginService
    {
        public Task<LoginResult> Login(string username, string password);
    }

    public interface IRegistrationService
    {
        public Task<RegistrationResult> Register(string username, string firstName, string lastName, string password, string repeatPassword, string email, string phone);
    }

    public interface ITokenService
    {
        string GetJsonWebTokenString(User user);
    }

    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly ILogger<LoginService> _logger;

        public LoginService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, ITokenService tokenService, ILogger<LoginService> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<LoginResult> Login(string username, string password)
        {
            var userResult = await _userRepository.GetUserByName(username);
            if (!userResult.IsSuccess)
                return new LoginResult { IsSuccess = false, ErrorMessage = userResult.ErrorMessage };

            var user = userResult.SelectedUser;
            var verifyPasswordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (verifyPasswordResult == PasswordVerificationResult.Failed)
                return new LoginResult { IsSuccess = false, ErrorMessage = "Incorrect password" };

            string tokenString = _tokenService.GetJsonWebTokenString(user);
            return new LoginResult { IsSuccess = true, TokenString = tokenString };
        }
    }

    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private readonly IConfiguration _configuration;

        public TokenService(ILogger<TokenService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public string GetJsonWebTokenString(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            var jwtToken = new JwtSecurityToken
                (
                   issuer: _configuration["JwtParams:Issuer"],
                   audience: _configuration["JwtParams:Audience"],
                   claims: claims,
                   expires: DateTime.UtcNow.AddMinutes(30),
                   signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtParams:SecretKey"])), SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }

    public class RegistrationService : IRegistrationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<RegistrationService> _logger;
        private readonly IPasswordHasher<User> _passwordHasher;

        public RegistrationService(IUserRepository userRepository, ITokenService tokenService, ILogger<RegistrationService> logger, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        private bool isPassowordsVaild(string password, string repeatPassword) => password == repeatPassword && password.Length >= 6;

        public async Task<RegistrationResult> Register(string username, string firstName, string lastName, string password, string repeatPassword, string email, string phone)
        {
            if (!isPassowordsVaild(password, password))
                return new RegistrationResult { IsSuccess = false, ErrorMessage = "Password is incorrect"};

            var existingUserResult = await _userRepository.GetUserByName(username);
            if (existingUserResult.IsSuccess)
                return new RegistrationResult { IsSuccess = false, ErrorMessage = $"User by {username} is exists" };

            string passwordHash = _passwordHasher.HashPassword(null, password);
            var userCreationResult = await _userRepository.CreateUser(username, passwordHash, firstName, lastName, email, phone);
            if (!userCreationResult.IsSuccess) 
                return new RegistrationResult { IsSuccess = false, ErrorMessage = userCreationResult.ErrorMessage };

            string tokenString = _tokenService.GetJsonWebTokenString(userCreationResult.SelectedUser);
            return new RegistrationResult { IsSuccess = true, TokenString = tokenString };
        }
    }
}