namespace Tweetz.MicroServices.LiveService.Controllers
{
    [ApiController]
    [Route("api/v1/live/[controller]")]
    public class StatisticController : ControllerBase
    {
        private readonly IStatisticRepository _statisticRepo;
        private readonly ILogger<StatisticController> _logger;

        public StatisticController(IStatisticRepository statisticRepo, ILogger<StatisticController> logger)
        {
            _statisticRepo = statisticRepo;
            _logger = logger;
        }

        // GET api/v1/live/statistic?range=7d
        [HttpGet]
        public async Task<IActionResult> GetStatistics([FromQuery] string range = "7d")
        {
            DateTime start;
            DateTime end = DateTime.UtcNow;
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);


            switch (range.ToLower())
            {
                case "day":
                    start = DateTime.UtcNow.Date;
                    break;
                case "yesterday":
                    start = DateTime.UtcNow.AddDays(-1);
                    break;
                case "7d":
                    start = DateTime.UtcNow.AddDays(-7);
                    break;
                case "month":
                    start = DateTime.UtcNow.AddMonths(-1);
                    break;
                case "3m":
                    start = DateTime.UtcNow.AddMonths(-3);
                    break;
                case "year":
                    start = DateTime.UtcNow.AddYears(-1);
                    break;
                default:
                    return BadRequest("Invalid range. Use 'day', '7d', 'month', or 'year'.");
            }

            var stats = await _statisticRepo.GetStatisticsAsync(userId, start, end);

            var grouped = range.ToLower() switch
            {
                "7d" => stats
                    .GroupBy(s => s.StartTime.Date) // group by jour
                    .Select(g => new
                    {
                        Period = g.Key.ToString("yyyy-MM-dd"),
                        UniqueViewers = g.Sum(x => x.UniqueViewers),
                        TotalDuration = TimeSpan.FromSeconds(g.Sum(x => x.TotalDuration.TotalSeconds))
                    }),

                "yesterday" => stats
                    .Where(s => s.StartTime.Date == DateTime.UtcNow.AddDays(-1).Date)
                    .Select(g => new
                    {
                        Period = g.StartTime.AddHours(2).ToString("yyyy-MM-dd HH:mm"),
                        UniqueViewers = g.UniqueViewers,
                        TotalDuration = g.TotalDuration
                    }),

                "3m" => stats
                    .GroupBy(s => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                        s.StartTime,
                        CalendarWeekRule.FirstDay,
                        DayOfWeek.Monday))
                    .Select(g => new
                    {
                        Period = $"Semaine {g.Key}",
                        UniqueViewers = g.Sum(x => x.UniqueViewers),
                        TotalDuration = TimeSpan.FromSeconds(g.Sum(x => x.TotalDuration.TotalSeconds))
                    }),

                "month" => stats
                    .GroupBy(s => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                        s.StartTime,
                        CalendarWeekRule.FirstDay,
                        DayOfWeek.Monday))
                    .Select(g => new
                    {
                        Period = $"Semaine {g.Key}",
                        UniqueViewers = g.Sum(x => x.UniqueViewers),
                        TotalDuration = TimeSpan.FromSeconds(g.Sum(x => x.TotalDuration.TotalSeconds))
                    }),

                "year" => stats
                    .GroupBy(s => new { s.StartTime.Year, s.StartTime.Month })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        UniqueViewers = g.Sum(x => x.UniqueViewers),
                        TotalDuration = TimeSpan.FromSeconds(g.Sum(x => x.TotalDuration.TotalSeconds))
                    }),

                _ => stats.Select(s => new
                {
                    Period = s.StartTime.AddHours(2).ToString("yyyy-MM-dd HH:mm"),
                    s.UniqueViewers,
                    s.TotalDuration
                })
            };

            return Ok(grouped);

        }
    }
}
