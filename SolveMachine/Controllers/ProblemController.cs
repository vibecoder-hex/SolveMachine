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
        private readonly ISelectionProblemRepository _selectionRepository;
        private readonly IModificationProblemRepository _modificationRepository;

        public ProblemController(ISelectionProblemRepository selectionRepository, IModificationProblemRepository modificationRepository)
        {
            _selectionRepository = selectionRepository;
            _modificationRepository = modificationRepository;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProblemCreationDto dto)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var existingProblem = await _selectionRepository.GetProblemByName(dto.Name, int.Parse(userIdClaim));
            if (existingProblem != null)
                return BadRequest(new { Error = "Problem already exists" });

            await _modificationRepository.CreateProblem(
                dto.Name,
                dto.Description,
                dto.DeadLineDate,
                dto.XCoord,
                dto.YCoord,
                dto.Priority,
                dto.Status,
                int.Parse(userIdClaim)
            );

            return Ok(new  { Message = "Problem created successfully" });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var problems = await _selectionRepository.GetAllProblems(int.Parse(userIdClaim));

            if (problems.Count == 0)
                return BadRequest(new { Error = "Problems list is empty"});

            return Ok(new { Problems = problems});
        }

        [Authorize]
        [HttpGet("{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var problem = await _selectionRepository.GetProblemByName(name, int.Parse(userIdClaim));

            if (problem == null)
                return BadRequest(new  { Error = "Problem not found" });

            return Ok(problem);
        }

        [Authorize]
        [HttpPost("filtered")]
        public async Task<IActionResult> FilteredGet([FromBody] ProblemFilteringDto dto)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var problems = await _selectionRepository.GetFilteredProblems(
                int.Parse(userIdClaim),
                dto.DeadLineDate,
                dto.CreationDate,
                dto.Priority,
                dto.Status);
            
            if (problems.Count == 0)
                return BadRequest(new  { Error = "Problems not found" });

            return Ok(new { Problems = problems });
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProblemUpdatingDto dto)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var existingProblem = await _selectionRepository.GetProblemByName(dto.Name, int.Parse(userIdClaim));
            if (existingProblem == null)
                return BadRequest(new { Error = "Problem not found" });
            await _modificationRepository.UpdateProblem(
                existingProblem,
                dto.Name,
                dto.Description,
                dto.DeadLineDate,
                dto.XCoord,
                dto.YCoord,
                dto.Priority,
                dto.Status);
            return Ok(new  { Message = "Problem updated successfully" });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var existingProblem = await _selectionRepository.GetProblem(id, int.Parse(userIdClaim));
            if  (existingProblem == null)
                return BadRequest(new { Error = "Problem not found" });

            await _modificationRepository.DeleteProblem(existingProblem);

            return Ok(new { Message = "Problem deleted successfully" });
        }
    }
}