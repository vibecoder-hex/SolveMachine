using Microsoft.AspNetCore.Mvc;
using SolveMachine.Models;

namespace SolveMachine.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginData)
        {
            _logger.LogInformation("Login is successful");
            return Ok(new { Message = "Login successful" });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto registerDto)
        {
            _logger.LogInformation("Register is successful");
            return Ok(new { Message = "Register successful" });
        }

        [HttpGet("profile")]
        public IActionResult Profile()
        {
            _logger.LogInformation("Profile is successful");
            return Ok(new { Message = "Profile successful" });
        }
    }
}