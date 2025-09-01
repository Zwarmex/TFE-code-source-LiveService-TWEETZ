namespace Tweetz.MicroServices.LiveService.Interfaces
{
    public interface IModerationRepository
    {
        Task<List<ModerationLog>> GetBanUserAsync(int userId);
        Task UnbanUserAsync(Guid logId, int streamerId);
    }
}