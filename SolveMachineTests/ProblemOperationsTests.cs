using Moq;
using SolveMachine.Controllers;
using SolveMachine.Models;
using SolveMachine.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SolveMachineTests
{
    public class ProblemOperationsTests
    {
        private readonly Mock<ISelectionProblemRepository> _selectionRepositoryMock;
        private readonly Mock<IModificationProblemRepository> _modificationRepositoryMock;
        private readonly ProblemController _problemController;
        private const int TestUserId = 1;
        private const string UserIdClaimValue = "1";

        public ProblemOperationsTests()
        {
            _selectionRepositoryMock = new Mock<ISelectionProblemRepository>();
            _modificationRepositoryMock = new Mock<IModificationProblemRepository>();
            _problemController = new ProblemController(_selectionRepositoryMock.Object, _modificationRepositoryMock.Object);

            SetupAuthenticatedUser();
        }

        private void SetupAuthenticatedUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, UserIdClaimValue)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _problemController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region Create Tests

        [Fact]
        public async Task CreateProblem_Success()
        {
            // Arrange
            var dto = new ProblemCreationDto(
                Name: "New Problem",
                Description: "Test Description",
                DeadLineDate: DateTime.Now.AddDays(7),
                XCoord: 100,
                YCoord: 200,
                Priority: ProblemPriority.High,
                Status: ProblemStatus.NotStarted
            );

            _selectionRepositoryMock.Setup(repo => repo.GetProblemByName(dto.Name, TestUserId))
                .ReturnsAsync((Problem?)null);
            _modificationRepositoryMock.Setup(repo => repo.CreateProblem(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<ProblemPriority>(),
                    It.IsAny<ProblemStatus>(),
                    It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _problemController.Post(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _modificationRepositoryMock.Verify(
                repo => repo.CreateProblem(
                    dto.Name,
                    dto.Description,
                    dto.DeadLineDate,
                    dto.XCoord,
                    dto.YCoord,
                    dto.Priority,
                    dto.Status,
                    TestUserId),
                Times.Once);
        }

        [Fact]
        public async Task CreateProblem_WhenProblemAlreadyExists()
        {
            // Arrange
            var existingProblem = new Problem
            {
                Id = 1,
                Name = "Existing Problem",
                Description = "Already exists",
                CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                DeadlineDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                DisplayXcoord = 100,
                DisplayYcoord = 200,
                Priority = ProblemPriority.High,
                Status = ProblemStatus.NotStarted,
                UserId = TestUserId
            };

            var dto = new ProblemCreationDto(
                Name: existingProblem.Name,
                Description: existingProblem.Description,
                DeadLineDate: DateTime.Now.AddDays(7),
                XCoord: 100,
                YCoord: 200,
                Priority: ProblemPriority.High,
                Status: ProblemStatus.NotStarted
            );

            _selectionRepositoryMock.Setup(repo => repo.GetProblemByName(dto.Name, TestUserId))
                .ReturnsAsync(existingProblem);

            // Act
            var result = await _problemController.Post(dto);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);
            _modificationRepositoryMock.Verify(repo => repo.CreateProblem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProblemPriority>(), It.IsAny<ProblemStatus>(), It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region Read Tests

        [Fact]
        public async Task GetAllProblems_Success()
        {
            // Arrange
            var problems = new List<Problem>
            {
                new Problem
                {
                    Id = 1,
                    Name = "Problem 1",
                    Description = "Description 1",
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                    DeadlineDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                    DisplayXcoord = 100,
                    DisplayYcoord = 200,
                    Priority = ProblemPriority.High,
                    Status = ProblemStatus.NotStarted,
                    UserId = TestUserId
                },
                new Problem
                {
                    Id = 2,
                    Name = "Problem 2",
                    Description = "Description 2",
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                    DeadlineDate = DateOnly.FromDateTime(DateTime.Now.AddDays(14)),
                    DisplayXcoord = 150,
                    DisplayYcoord = 250,
                    Priority = ProblemPriority.Medium,
                    Status = ProblemStatus.InProccess,
                    UserId = TestUserId
                }
            };

            _selectionRepositoryMock.Setup(repo => repo.GetAllProblems(TestUserId))
                .ReturnsAsync(problems);

            // Act
            var result = await _problemController.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetAllProblems_EmptyList()
        {
            // Arrange
            var emptyList = new List<Problem>();
            _selectionRepositoryMock.Setup(repo => repo.GetAllProblems(TestUserId))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _problemController.Get();

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);
        }

        [Fact]
        public async Task GetProblemByName_Success()
        {
            // Arrange
            var problem = new Problem
            {
                Id = 1,
                Name = "Test Problem",
                Description = "Test Description",
                CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                DeadlineDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                DisplayXcoord = 100,
                DisplayYcoord = 200,
                Priority = ProblemPriority.High,
                Status = ProblemStatus.NotStarted,
                UserId = TestUserId
            };

            _selectionRepositoryMock.Setup(repo => repo.GetProblemByName("Test Problem", TestUserId))
                .ReturnsAsync(problem);

            // Act
            var result = await _problemController.GetByName("Test Problem");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetProblemByName_NotFound()
        {
            // Arrange
            _selectionRepositoryMock.Setup(repo => repo.GetProblemByName("NonExistent", TestUserId))
                .ReturnsAsync((Problem?)null);

            // Act
            var result = await _problemController.GetByName("NonExistent");

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);
        }

        [Fact]
        public async Task GetFilteredProblems_Success()
        {
            // Arrange
            var filteredProblems = new List<Problem>
            {
                new Problem
                {
                    Id = 1,
                    Name = "High Priority Problem",
                    Description = "High priority task",
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                    DeadlineDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                    DisplayXcoord = 100,
                    DisplayYcoord = 200,
                    Priority = ProblemPriority.High,
                    Status = ProblemStatus.NotStarted,
                    UserId = TestUserId
                }
            };

            var filterDto = new ProblemFilteringDto(
                Name: null,
                DeadLineDate: null,
                Priority: ProblemPriority.High,
                Status: null,
                CreationDate: null
            );

            _selectionRepositoryMock.Setup(repo => repo.GetFilteredProblems(
                    TestUserId,
                    filterDto.DeadLineDate,
                    filterDto.CreationDate,
                    filterDto.Priority,
                    filterDto.Status))
                .ReturnsAsync(filteredProblems);

            // Act
            var result = await _problemController.FilteredGet(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetFilteredProblems_NoResults()
        {
            // Arrange
            var emptyList = new List<Problem>();

            var filterDto = new ProblemFilteringDto(
                Name: null,
                DeadLineDate: DateOnly.FromDateTime(DateTime.Now.AddYears(5)),
                Priority: null,
                Status: null,
                CreationDate: null
            );

            _selectionRepositoryMock.Setup(repo => repo.GetFilteredProblems(
                    TestUserId,
                    filterDto.DeadLineDate,
                    filterDto.CreationDate,
                    filterDto.Priority,
                    filterDto.Status))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _problemController.FilteredGet(filterDto);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateProblem_Success()
        {
            // Arrange
            var existingProblem = new Problem
            {
                Id = 1,
                Name = "Old Name",
                Description = "Old Description",
                CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                DeadlineDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                DisplayXcoord = 100,
                DisplayYcoord = 200,
                Priority = ProblemPriority.High,
                Status = ProblemStatus.NotStarted,
                UserId = TestUserId
            };

            var updateDto = new ProblemUpdatingDto(
                Name: "Updated Name",
                Description: "Updated Description",
                DeadLineDate: DateTime.Now.AddDays(14),
                XCoord: 150,
                YCoord: 250,
                Priority: ProblemPriority.Medium,
                Status: ProblemStatus.InProccess
            );

            _selectionRepositoryMock.Setup(repo => repo.GetProblemByName(updateDto.Name, TestUserId))
                .ReturnsAsync(existingProblem);
            _modificationRepositoryMock.Setup(repo => repo.UpdateProblem(
                    It.IsAny<Problem>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<ProblemPriority?>(),
                    It.IsAny<ProblemStatus?>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _problemController.Update(existingProblem.Id, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _modificationRepositoryMock.Verify(repo => repo.UpdateProblem(
                    It.IsAny<Problem>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<ProblemPriority?>(),
                    It.IsAny<ProblemStatus?>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProblem_NotFound()
        {
            // Arrange
            var updateDto = new ProblemUpdatingDto(
                Name: "NonExistent",
                Description: "New Description",
                DeadLineDate: null,
                XCoord: null,
                YCoord: null,
                Priority: null,
                Status: null
            );

            _selectionRepositoryMock.Setup(repo => repo.GetProblemByName(updateDto.Name, TestUserId))
                .ReturnsAsync((Problem?)null);

            // Act
            var result = await _problemController.Update(999, updateDto);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);
            _modificationRepositoryMock.Verify(repo => repo.UpdateProblem(It.IsAny<Problem>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<ProblemPriority?>(), It.IsAny<ProblemStatus?>()), Times.Never);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteProblem_Success()
        {
            // Arrange
            var problemToDelete = new Problem
            {
                Id = 1,
                Name = "Problem to Delete",
                Description = "This will be deleted",
                CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                DeadlineDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                DisplayXcoord = 100,
                DisplayYcoord = 200,
                Priority = ProblemPriority.High,
                Status = ProblemStatus.NotStarted,
                UserId = TestUserId
            };

            _selectionRepositoryMock.Setup(repo => repo.GetProblem(TestUserId, 1))
                .ReturnsAsync(problemToDelete);
            _modificationRepositoryMock.Setup(repo => repo.DeleteProblem(It.IsAny<Problem>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _problemController.Delete(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _modificationRepositoryMock.Verify(repo => repo.DeleteProblem(It.IsAny<Problem>()), Times.Once);
        }

        [Fact]
        public async Task DeleteProblem_NotFound()
        {
            // Arrange
            _selectionRepositoryMock.Setup(repo => repo.GetProblem(TestUserId, 999))
                .ReturnsAsync((Problem?)null);

            // Act
            var result = await _problemController.Delete(999);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);
            _modificationRepositoryMock.Verify(repo => repo.DeleteProblem(It.IsAny<Problem>()), Times.Never);
        }

        #endregion
    }
}
