using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolveMachine.Models;
using SolveMachine.Repositories;
using SolveMachine.Services;

namespace SolveMachine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly ILoginService _loginService;
        private readonly IRegistrationService _registrationService;
        private readonly IUserRepository _userRepository;

        public UserController(ILogger<UserController> logger, ILoginService loginService, IRegistrationService registrationService, IUserRepository userRepository)
        {
            _logger = logger;
            _loginService = loginService;
            _registrationService = registrationService;
            _userRepository = userRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginData)
        {
            var loginResult = await _loginService.Login(loginData.Username, loginData.Password);

            if (!loginResult.IsSuccess)
                return Unauthorized(new { Error = loginResult.ErrorMessage });

            return Ok(new { Token = loginResult.TokenString });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var registrationResult = await _registrationService
                .Register(registerDto.Username,
                        registerDto.FirstName,
                        registerDto.LastName,
                        registerDto.Password,
                        registerDto.RepeatPassword,
                        registerDto.Email,
                        registerDto.Phone);

            if (!registrationResult.IsSuccess)
                return Unauthorized(new { Error = registrationResult.ErrorMessage });

            return Ok(new { Token = registrationResult.TokenString });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            string? username = HttpContext.User?.Identity.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { Error = $"username {username} does not exists in http context" });

            var user = await _userRepository.GetUserByName(username);
            if (user == null)
                return Unauthorized(new {Error = $"User by username {username} does not exists"});
            
            var profile = new ProfileDto(user.Username, user.FirstName, user.LastName, user.Email, user.Phone, user.CreatedAt);
            return Ok(profile);
        }
    }
}