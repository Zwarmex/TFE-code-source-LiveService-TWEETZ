namespace Tweetz.MicroServices.LiveService.Interfaces
{
    public interface ILiveRepository
    {

        Task<List<Live>> GetAllAsync();
        Task<Live?> GetByIdAsync(string apiVideoLiveStreamId);
        Task CreateAsync(Live live, string streamerId);
        Task UpdateAsync(string apiVideoLiveStreamId, LiveUpdateDto live, string streamerId);
        Task DeleteAsync(string apiVideoLiveStreamId, string streamerId);
        Task UploadThumbnailAsync(string apiVideoLiveStreamId, string thumbnail);
        Task DeleteThumbnailAsync(string apiVideoLiveStreamId, string thumbnailUrl);
        Task CompleteLiveStreamAsync(string apiVideoLiveStreamId);
    }
}