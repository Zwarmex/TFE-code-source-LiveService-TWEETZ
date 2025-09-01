namespace Tweetz.MicroServices.LiveService.Interfaces
{
    public interface ILiveRepository
    {

        Task<List<Live>> GetAllAsync();
        Task<List<Live>> GetAllOnStreamAsync();
        Task<Live?> GetByIdAsync(string apiVideoLiveStreamId, int userId);
        Task<string> CreateAsync(LiveCreateDto live, int streamerId, string streamerUsername);
        Task UpdateAsync(string apiVideoLiveStreamId, LiveUpdateDto live, int streamerId);
        Task DeleteAsync(string apiVideoLiveStreamId, int streamerId);
        Task UploadThumbnailAsync(string apiVideoLiveStreamId, IFormFile thumbnail);
        Task DeleteThumbnail(string apiVideoLiveStreamId, int streamerId);
        Task CompleteLiveStreamAsync(string apiVideoLiveStreamId);
        Task<List<Live>> GetByStreamerIdAsync(int streamerId);
        Task<int> GetViewerCountAsync(string apiVideoLiveStreamId);
    }
}