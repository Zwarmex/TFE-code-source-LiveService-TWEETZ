namespace Tweetz.MicroServices.LiveService.Models
{
    [Table("MODERATION_LOGS")]
    public class ModerationLog
    {
        [Key]
        [Column("LOG_Id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("LIVE_ApiVideoLiveStreamId")]
        public string LiveId { get; set; } = string.Empty;

        [Required]
        [Column("LOG_ModeratorId")]
        public int ModeratorId { get; set; }
        [Required]
        [Column("LOG_StreamerId")]
        public int StreamerId { get; set; }

        [Required]
        [Column("LOG_TargetUserId")]
        public int TargetUserId { get; set; }
        [Required]
        [Column("LOG_TargetUsername")]
        public string TargetUsername { get; set; } = string.Empty;

        [Required]
        [Column("LOG_ActionType")]
        public string ActionType { get; set; } = string.Empty; // DeleteMessage, Timeout, Ban

        [Column("LOG_DurationSeconds")]
        public int? DurationSeconds { get; set; }

        [Required]
        [Column("LOG_ActionDate")]
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;
    }
}
