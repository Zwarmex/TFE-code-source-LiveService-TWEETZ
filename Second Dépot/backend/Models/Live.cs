namespace Tweetz.MicroServices.LiveService.Models
{
    [Table("LIVE")]
    public class Live
    {
        [Key]
        [Column("LIVE_ApiVideoLiveStreamId")]
        public string ApiVideoLiveStreamId { get; set; } = string.Empty;
        [Required]
        [Column("LIVE_IsPublic")]
        public bool IsPublic { get; set; }
        [Required]
        [Column("LIVE_Title")]
        public string Title { get; set; } = string.Empty;
        [Required]

        [Column("LIVE_StreamerId")]
        public int StreamerId { get; set; }

        [Column("LIVE_StreamerUsername")]
        public string StreamerUsername { get; set; } = string.Empty;

        [Column("LIVE_Description")]
        public string? Description { get; set; } = string.Empty;
        [Column("LIVE_StartTime")]
        public DateTime? StartTime { get; set; }

        [Column("LIVE_EndTime")]
        public DateTime? EndTime { get; set; }
        [Required]
        [Column("LIVE_Broadcasting")]
        public bool? Broadcasting { get; set; } = false;
        [Required]
        [Column("LIVE_ApiVideoStreamKey")]
        public string? ApiVideoStreamKey { get; set; } = string.Empty;
        [Column("LIVE_IsTerminated")]
        public bool? IsTerminated { get; set; } = false;
        [Required]

        [Column("LIVE_CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("LIVE_UpdatedAt")]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        [Column("LIVE_ThumbnailUrl")]
        public string? ThumbnailUrl { get; set; }
        [Column("LIVE_InvitedUserId")]
        public int? InvitedUserId { get; set; }
        [NotMapped]
        public string? PlayerUrl { get; set; }
    }
}