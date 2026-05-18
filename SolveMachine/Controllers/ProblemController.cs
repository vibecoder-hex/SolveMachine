using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolveMachine.Models;
using SolveMachine.Repositories;
using System.Security.Claims;

namespace SolveMachine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProblemController : ControllerBase
    {
        private readonly ISelectionProblemRepository _selectionProblemRepository;
        private readonly IModificationProblemRepository _modificationProblemRepository;

        public ProblemController(ISelectionProblemRepository selectionProblemRepository, IModificationProblemRepository modificationProblemRepository)
        {
            _selectionProblemRepository = selectionProblemRepository;
            _modificationProblemRepository = modificationProblemRepository;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProblemCreationDto dto)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var creationProblemResult = await _modificationProblemRepository.CreateProblem(
                dto.Name,
                dto.Description,
                dto.DeadLineDate,
                dto.XCoord,
                dto.YCoord,
                dto.Priority,
                dto.Status,
                int.Parse(userIdClaim)
            );

            if (!creationProblemResult.IsSuccess)
                return BadRequest(new { Error = creationProblemResult.ErrorMessage });

            return Ok(creationProblemResult.Problem);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var problemGettingResult = await _selectionProblemRepository.GetAllProblems(int.Parse(userIdClaim));

            if (!problemGettingResult.IsSuccess)
                return BadRequest(new { Error = problemGettingResult.ErrorMessage });

            return Ok(new { Problems = problemGettingResult.Problems });
        }

        [Authorize]
        [HttpGet("{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var problemGettingResult = await _selectionProblemRepository.GetProblemByName(name, int.Parse(userIdClaim));

            if (!problemGettingResult.IsSuccess)
                return BadRequest(new { Error = problemGettingResult.ErrorMessage });

            return Ok(problemGettingResult.Problem);
        }

        [Authorize]
        [HttpPost("filtered")]
        public async Task<IActionResult> FilteredGet([FromBody] ProblemFilteringDto dto)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var problemGettingResult = await _selectionProblemRepository.GetFilteredProblems(
                int.Parse(userIdClaim),
                dto.DeadLineDate,
                dto.CreationDate,
                dto.Priority,
                dto.Status);
            if (!problemGettingResult.IsSuccess)
                return BadRequest(new { Error = problemGettingResult.ErrorMessage });

            return Ok(new { Problems = problemGettingResult.Problems });
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProblemUpdatingDto dto)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var problemUpdatingResult = await _modificationProblemRepository.UpdateProblem(
                int.Parse(userIdClaim),
                id,
                dto.Name,
                dto.Description,
                dto.DeadLineDate,
                dto.XCoord,
                dto.YCoord,
                dto.Priority,
                dto.Status);
            if (!problemUpdatingResult.IsSuccess)
                return BadRequest(new { Error = problemUpdatingResult.ErrorMessage });

            return Ok(problemUpdatingResult.Problem);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var problemDeletingResult = await _modificationProblemRepository.DeleteProblem(int.Parse(userIdClaim), id);

            if (!problemDeletingResult.IsSuccess)
                return BadRequest(new { Error = problemDeletingResult.ErrorMessage });

            return Ok(new { Message = "Problem deleted successfully" });
        }
    }
}
