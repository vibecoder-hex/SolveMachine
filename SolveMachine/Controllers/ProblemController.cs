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
        private readonly IProblemRepository _problemRepository;

        public ProblemController(IProblemRepository problemRepository)
        {
            _problemRepository = problemRepository;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProblemCreationDto dto)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var creationProblemResult = await _problemRepository.CreateProblem(
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
            var problemGettingResult = await _problemRepository.GetAllProblems(int.Parse(userIdClaim));

            if (!problemGettingResult.IsSuccess)
                return BadRequest(new { Error = problemGettingResult.ErrorMessage });

            return Ok(new { Problems = problemGettingResult.Problems });
        }

        [Authorize]
        [HttpPost("filtered")]
        public async Task<IActionResult> FilteredGet([FromBody] ProblemFilteringDto dto)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var problemGettingResult = await _problemRepository.GetFilteredProblems(
                int.Parse(userIdClaim),
                dto.DeadLineDate,
                dto.CreationDate,
                dto.Priority,
                dto.Status);
            if (!problemGettingResult.IsSuccess)
                return BadRequest(new { Error = problemGettingResult.ErrorMessage });

            return Ok(new { Problems = problemGettingResult.Problems });
        }
    }
}
