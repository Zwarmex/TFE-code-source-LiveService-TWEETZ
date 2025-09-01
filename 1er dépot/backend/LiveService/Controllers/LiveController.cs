
namespace Tweetz.MicroServices.LiveService.Controllers
{
    [ApiController]
    [Route("api/v1/content/[controller]")]
    public class LiveController : ControllerBase
    {
        private readonly ILiveRepository _liveRepo;

        public LiveController(ILiveRepository liveRepo)
        {
            _liveRepo = liveRepo;
        }

        [HttpGet]
        [Route("get-all-async")]
        public async Task<IActionResult> GetAllAsync()
        {
            var lives = await _liveRepo.GetAllAsync();
            var liveDto = lives.Select(l => l.ToLiveDto()).ToList();
            return Ok(liveDto);
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            var live = await _liveRepo.GetByIdAsync(id);
            if (live == null)
                return NotFound();

            return Ok(live.ToLiveDto());
        }
        [HttpDelete]
        [Route("{apiVideoLiveStreamId}")]

        public async Task<IActionResult> DeleteAsync(string apiVideoLiveStreamId)
        {
            await _liveRepo.DeleteAsync(apiVideoLiveStreamId, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return NoContent();
        }
        [HttpPatch]
        [Route("{apiVideoLiveStreamId}")]
        public async Task<IActionResult> UpdateAsync(string apiVideoLiveStreamId, [FromBody] LiveUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var live = await _liveRepo.GetByIdAsync(apiVideoLiveStreamId);
            if (live == null)
                return NotFound();

            live.Name = dto.Title;
            live.IsPublic = dto.IsPublic;
            live.PlayerUrl = dto.PlayerUrl;
            live.Broadcasting = dto.Broadcasting;

            await _liveRepo.UpdateAsync(apiVideoLiveStreamId, dto, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return NoContent();
        }

        [HttpPost]
        [Route("create-async")]
        public async Task<IActionResult> CreateAsync([FromBody] LiveCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var live = new Live
            {
                Name = dto.Title,
                IsPublic = dto.IsPublic,
                PlayerUrl = dto.PlayerUrl,
                Broadcasting = dto.Broadcasting,
                ThumbnailUrl = dto.ThumbnailUrl
            };

            try
            {
                // Call the repository to create the live stream
                await _liveRepo.CreateAsync(live, dto.StreamerId);

                // Return the created live stream with its ID
                return CreatedAtAction(nameof(GetByIdAsync), new { id = live.ApiVideoLiveStreamId }, live.ToLiveDto());
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
        [HttpPost]
        [Route("{apiVideoLiveStreamId}/thumbnail")]
        public async Task<IActionResult> UploadThumbnailAsync(string apiVideoLiveStreamId, string thumbnail)
        {
            if (string.IsNullOrEmpty(thumbnail))
                return BadRequest("No file uploaded.");

            await _liveRepo.UploadThumbnailAsync(apiVideoLiveStreamId, thumbnail);
            return NoContent();
        }
        [HttpDelete]
        [Route("{apiVideoLiveStreamId}/thumbnail")]
        public async Task<IActionResult> DeleteThumbnailAsync(string apiVideoLiveStreamId, string thumbnailUrl)
        {
            if (string.IsNullOrEmpty(thumbnailUrl))
                return BadRequest("No thumbnail URL provided.");

            await _liveRepo.DeleteThumbnailAsync(apiVideoLiveStreamId, thumbnailUrl);
            return NoContent();
        }
        [HttpPut]
        [Route("{apiVideoLiveStreamId}/complete")]
        public async Task<IActionResult> CompleteLiveStreamAsync(string apiVideoLiveStreamId)
        {
            await _liveRepo.CompleteLiveStreamAsync(apiVideoLiveStreamId);
            return Accepted();
        }
    }
}