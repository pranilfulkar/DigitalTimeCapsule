using System.Security.Claims;
using CapsuleAPI.DTO;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapsuleAPI.MediatRHandlers;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt; 

namespace CapsuleAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CapsuleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CapsuleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateCapsule([FromForm] CapsuleDto capsuleDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("No valid user ID found in token.");
            }

            var result = await _mediator.Send(new CreateCapsuleCommand(capsuleDto, userId));

            return result
            ? Ok(new { message = "Time capsule created successfully." })
            : BadRequest(new { message = "Failed to create capsule." });
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetUserCapsules(
            [FromQuery] string sortBy = "unlockDate",
            [FromQuery] string sortDirection = "desc",
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)

        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("No valid user ID found in token.");

            var (capsules, totalCount) = await _mediator.Send(new GetUserCapsulesQuery(userId, sortBy, sortDirection, pageIndex, pageSize));

            return Ok(new
            {
                capsules,
                totalCount
            });
        }

        [HttpPost("inviteCollaborator")]
        public async Task<IActionResult> InviteCollaborator([FromBody] InviteContributorDTO inviteDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("No valid user ID found in token.");

            var result = await _mediator.Send(new InviteContributorCommand(inviteDto, userId));
            return result ? Ok(new { message = "Invited Contributor successfully!" }) : BadRequest(new{message="Failed to invite."});
        }

        [HttpPost("contribute")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddContribution([FromForm] ContributionDto contributionDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("No valid user ID found in token.");

            string imageUrl = null;

            if (contributionDto.ImageFile != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}_{contributionDto.ImageFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await contributionDto.ImageFile.CopyToAsync(stream);
                }

                imageUrl = $"/Uploads/{fileName}";
            }

            var result = await _mediator.Send(new AddContributionCommand(contributionDto, userId, imageUrl));
            return result ? Ok(new { message = "Contribution Added Successfully" }) : BadRequest(new { message = "Failed to add contribution" });
        }
        [HttpPost("seal")]
        public async Task<IActionResult> SealCapsule([FromBody] SealCapsuleDto sealDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("No valid user ID found in token.");

            var result = await _mediator.Send(new SealCapsuleCommand(sealDto.CapsuleId, sealDto.UnlockDate, userId));
            return result ? Ok(new { message = "Capsule sealed successfully!" }) 
                : BadRequest(new { message = "Failed to seal capsule." });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCapsuleDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("No valid user ID found in token.");

            var capsule = await _mediator.Send(new GetCapsuleDetailsQuery(id, userId));
            if (capsule == null)
                return Forbid("Currently you do not have access to this capsule or Wait for Unlock Date.");

            return Ok(capsule);
        }
    }
}