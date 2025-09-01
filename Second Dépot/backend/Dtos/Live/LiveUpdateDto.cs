namespace Tweetz.MicroServices.LiveService.Dtos.Live
{
    public class LiveUpdateDto
    {
        public bool IsPublic { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool Broadcasting { get; set; } = false;
        public bool IsTerminated { get; set; } = false;
        public int? InvitedUserId { get; set; } = null;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}