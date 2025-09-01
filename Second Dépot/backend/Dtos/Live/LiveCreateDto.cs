namespace Tweetz.MicroServices.LiveService.Dtos.Live
{
    public class LiveCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public int? InvitedUserId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}