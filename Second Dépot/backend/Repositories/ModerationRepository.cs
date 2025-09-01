

namespace Tweetz.MicroServices.LiveService.Repositories
{
    public class ModerationRepository : IModerationRepository
    {

        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ModerationRepository> _logger;

        public ModerationRepository(ApplicationDBContext context, IConfiguration configuration, ILogger<ModerationRepository> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<List<ModerationLog>> GetBanUserAsync(int userId)
        {
            var bannedUser = await _context.ModerationLogs
                    .Where(v => v.ActionType == "Ban" && v.StreamerId == userId)
                    .ToListAsync();
            return bannedUser;
        }

        public Task UnbanUserAsync(Guid logId, int streamerId)
        {
            var log = _context.ModerationLogs.FirstOrDefault(l => l.Id == logId && l.ActionType == "Ban" && l.StreamerId == streamerId);

            if (log == null)
            {
                return Task.FromResult<ModerationLog?>(null);
            }
            _context.ModerationLogs.Remove(log);
            _context.SaveChanges();
            return Task.FromResult<ModerationLog?>(log);
        }

    }
}