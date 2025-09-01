namespace Tweetz.MicroServices.LiveService.Repositories
{
    public class StatisticRepository : IStatisticRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<StatisticRepository> _logger;

        public StatisticRepository(ApplicationDBContext context, ILogger<StatisticRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<LiveStatistic>> GetStatisticsAsync(int userId, DateTime start, DateTime end)
        {
            try
            {
                return await _context.LiveStatistics
                    .Where(stat => stat.StartTime >= start && stat.EndTime <= end && stat.UserId == userId)
                    .OrderBy(stat => stat.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching statistics.");
                return Enumerable.Empty<LiveStatistic>().ToList();
            }
        }
    }
}
