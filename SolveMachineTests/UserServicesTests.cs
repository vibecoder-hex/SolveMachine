using Moq;
using SolveMachine.Repositories;
using Microsoft.AspNetCore.Identity;
using SolveMachine.Models;
using SolveMachine.Services;

namespace SolveMachineTests.UserServicesTests
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock = new Mock<IPasswordHasher<User>>();
        private readonly Mock<ITokenService> _tokenServiceMock = new Mock<ITokenService>();

        private ILoginService _loginService;
        private IRegistrationService _registrationService;

        public AuthenticationServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _loginService = new LoginService(_userRepositoryMock.Object, _passwordHasherMock.Object, _tokenServiceMock.Object);
            _registrationService = new RegistrationService(_userRepositoryMock.Object, _tokenServiceMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task LoginTest()
        {
            var user = new User
            {
                Username = "testuser",
                FirstName = "Simon",
                LastName = "Babushkin",
                PasswordHash = "hashedpassword",
                Email = "testuser@example.com",
                Phone = "1234567890",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = new DateOnly(1999, 1, 1)
            };
            _userRepositoryMock.Setup(repo => repo.GetUserByName("testuser"))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(repo => repo.VerifyHashedPassword(user, user.PasswordHash, "password"))
                .Returns(PasswordVerificationResult.Success);
            var result = await _loginService.Login("testuser", "password");
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task RegistrationTestWhenUserIsExists()
        {
            string username = "testuser";
            string password = "password";

            _userRepositoryMock.Setup(repo => repo.GetUserByName(username))
                .ReturnsAsync(new User { Username = username });

            var result = await _registrationService.Register(username, "Max", "Messenger", password, password, "biven@vtuz.su", "1234567890");
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task RegistrationTestsWhenUserNotExists()
        {
            string username = "testuser";
            string password = "password";
            string hashedPassword = "hashedpassword";
            string fakeToken = "fakeToken";

            _userRepositoryMock.Setup(repo => repo.GetUserByName(username))
                .ReturnsAsync((User?)null);
            _passwordHasherMock.Setup(repo => repo.HashPassword(It.IsAny<User>(), password))
                .Returns(hashedPassword);
            _userRepositoryMock.Setup(repo => repo.CreateUser(username, hashedPassword, "Max", "Messenger", "biven@vtuz.su", "1234567890"))
                .ReturnsAsync(new User { Username = username, PasswordHash = hashedPassword });
            _tokenServiceMock.Setup(service => service.GetJsonWebTokenString(It.IsAny<User>()))
                .Returns(fakeToken);

            var result = await _registrationService.Register(username, "Max", "Messenger", password, password, "biven@vtuz.su", "1234567890");
            Assert.True(result.IsSuccess);
        }
    }
}
