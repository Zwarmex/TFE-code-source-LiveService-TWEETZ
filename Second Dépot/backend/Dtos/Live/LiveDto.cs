namespace Tweetz.MicroServices.LiveService.Dtos.Live
{
    public class LiveDto
    {
        public string ApiVideoLiveStreamId { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public string? StreamKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int StreamerId { get; set; }
        public string StreamerUsername { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string PlayerUrl { get; set; } = string.Empty;
        public bool Broadcasting { get; set; } = false;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public int? InvitedUserId { get; set; }
    }
}