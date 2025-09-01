namespace Tweetz.MicroServices.LiveService.Models
{
    [Table("LIVE_STATISTICS")]
    public class LiveStatistic
    {
        [Key]
        [Column("STAT_Id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("STAT_ApiVideoLiveStreamId")]
        public string ApiVideoLiveStreamId { get; set; } = string.Empty;
        [Required]
        [Column("STAT_LiveTitle")]
        public string LiveTitle { get; set; } = string.Empty;
        [Required]
        [Column("STAT_IsPublic")]
        public bool IsPublic { get; set; } = false;
        [Required]
        [Column("STAT_UserId")]
        public int UserId { get; set; } = 0;
        [Column("STAT_UniqueViewers")]
        public int UniqueViewers { get; set; } = 0;
        [Column("STAT_MaxViewers")]
        public int MaxViewers { get; set; } = 0;
        [Column("STAT_UniqueChatters")]
        public int UniqueChatters { get; set; } = 0;
        [Column("STAT_TotalMessages")]
        public int TotalMessages { get; set; } = 0;
        [Column("STAT_StartTime")]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        [Column("STAT_EndTime")]
        public DateTime EndTime { get; set; } = DateTime.UtcNow;
        [NotMapped]
        public Dictionary<int, int> MessagesPerUser { get; set; } = new();
        [Column("STAT_TotalDuration")]
        public TimeSpan TotalDuration { get; set; } = TimeSpan.Zero;
        [Column("STAT_AvgWatchDuration")]
        public TimeSpan AvgWatchDuration { get; set; } = TimeSpan.Zero;
        [Column("STAT_MessagesPerUser")]
        [MaxLength(1000)]
        public string MessagesPerUserJson { get; set; } = "{}";
        [Column("STAT_CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
