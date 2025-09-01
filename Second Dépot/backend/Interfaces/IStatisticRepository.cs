namespace Tweetz.MicroServices.LiveService.Interfaces
{
    public interface IStatisticRepository
    {
        Task<List<LiveStatistic>> GetStatisticsAsync(int userId, DateTime start, DateTime end);
    }
}
