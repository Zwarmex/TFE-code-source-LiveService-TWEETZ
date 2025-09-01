namespace Tweetz.MicroServices.LiveService.Controllers
{

    [ApiController]
    [Route("api/v1/live/[controller]")]

    public class ModerationController : ControllerBase
    {
        private readonly IModerationRepository _moderationRepo;

        public ModerationController(IModerationRepository moderationRepo)
        {
            _moderationRepo = moderationRepo;
        }

        [HttpGet("ban")]
        public async Task<List<ModerationLog>> BanUser()
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
            var bannedUsers = await _moderationRepo.GetBanUserAsync(userId);
            return bannedUsers;
        }

        [HttpDelete("ban/{logId}")]
        public async Task<IActionResult> UnbanUser([FromRoute] Guid logId)
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
            await _moderationRepo.UnbanUserAsync(logId, userId);

            return NoContent();

        }

    }
}