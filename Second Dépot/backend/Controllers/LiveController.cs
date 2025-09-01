
namespace Tweetz.MicroServices.LiveService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LiveController : ControllerBase
    {
        private readonly ILiveRepository _liveRepo;

        public LiveController(ILiveRepository liveRepo)
        {
            _liveRepo = liveRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var lives = await _liveRepo.GetAllAsync();
            var liveDto = lives.Select(l => l.ToLiveDto()).ToList();
            return Ok(liveDto);
        }
        [HttpGet]
        [Route("on-stream")]
        public async Task<IActionResult> GetAllOnStreamAsync()
        {
            var lives = await _liveRepo.GetAllOnStreamAsync();
            var liveDto = lives.Select(l => l.ToLiveDto()).ToList();
            return Ok(liveDto);
        }
        [HttpGet]
        [Route("streamer")]
        public async Task<IActionResult> GetByStreamerIdAsync(int streamerId)
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
            var lives = await _liveRepo.GetByStreamerIdAsync(userId);
            var liveDto = lives.Select(l => l.ToLiveDto()).ToList();
            return Ok(liveDto);
        }
        [HttpGet]
        [Route("{apiVideoLiveStreamId}")]

        public async Task<IActionResult> GetByIdAsync(string apiVideoLiveStreamId)
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
            string username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value;
            var live = await _liveRepo.GetByIdAsync(apiVideoLiveStreamId, userId);
            if (live == null)
                return NotFound();
            var dto = live.ToLiveDto();

            return Ok(new { user = new { userId = userId, username = username }, live = dto });
        }
        [HttpDelete]
        [Route("{apiVideoLiveStreamId}")]
        public async Task<IActionResult> DeleteAsync(string apiVideoLiveStreamId)
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);

            await _liveRepo.DeleteAsync(apiVideoLiveStreamId, userId);
            return NoContent();
        }
        [HttpPatch]
        [Route("{apiVideoLiveStreamId}")]
        public async Task<IActionResult> UpdateAsync(string apiVideoLiveStreamId, [FromBody] LiveUpdateDto dto)
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var live = await _liveRepo.GetByIdAsync(apiVideoLiveStreamId, userId);
            if (live == null)
                return NotFound();

            await _liveRepo.UpdateAsync(apiVideoLiveStreamId, dto, userId);
            return NoContent();
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] LiveCreateDto dto)
        {
            try
            {
                int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
                string username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value;
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var live = new LiveCreateDto
                {
                    Title = dto.Title,
                    IsPublic = dto.IsPublic,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    InvitedUserId = dto.InvitedUserId
                };
                // Call the repository to create the live stream
                var res = await _liveRepo.CreateAsync(live, userId, username);
                return Ok(res);

                // Return the created live stream with its ID
            }
            catch (ApiException ex)
            {
                // Handle API-specific errors
                return StatusCode(400, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
        [HttpPost("{apiVideoLiveStreamId}/thumbnail")]
        public async Task<IActionResult> UploadThumbnailAsync(
            [FromRoute] string apiVideoLiveStreamId,
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Aucun fichier reçu.");

            // (Optionnel) valider le type MIME
            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowed.Contains(file.ContentType))
                return BadRequest("Format non supporté.");

            await _liveRepo.UploadThumbnailAsync(apiVideoLiveStreamId, file);
            return NoContent();
        }

        [HttpDelete]
        [Route("{apiVideoLiveStreamId}/thumbnail")]
        public async Task<IActionResult> DeleteThumbnailAsync(string apiVideoLiveStreamId)
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
            if (string.IsNullOrEmpty(apiVideoLiveStreamId))
            {
                return BadRequest("Invalid live stream ID.");
            }

            await _liveRepo.DeleteThumbnail(apiVideoLiveStreamId, userId);
            return NoContent();
        }
        [HttpPut]
        [Route("{apiVideoLiveStreamId}/complete")]
        public async Task<IActionResult> CompleteLiveStreamAsync(string apiVideoLiveStreamId)
        {
            await _liveRepo.CompleteLiveStreamAsync(apiVideoLiveStreamId);
            return Accepted();
        }
        [HttpGet]
        [Route("{apiVideoLiveStreamId}/viewers")]
        public async Task<IActionResult> GetViewerCountAsync(string apiVideoLiveStreamId)
        {
            var viewerCount = await _liveRepo.GetViewerCountAsync(apiVideoLiveStreamId);

            return Ok(new { viewers = viewerCount });
        }

    }
}