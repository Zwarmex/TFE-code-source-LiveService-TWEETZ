namespace Tweetz.MicroServices.LiveService.Models
{
    [Table("LIVE_VIEWERS")]
    public class LiveViewer
    {
        [Key]
        [Column("LIVI_Id")]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [ForeignKey("Live")]
        [Column("LIVI_ApiVideoLiveStreamId")]
        public string LiveId { get; set; } = string.Empty;
        [Required]
        [Column("LIVI_UserId")]
        public int UserId { get; set; }
        [Column("LIVI_JoinAt")]
        public DateTime JoinAt { get; set; } = DateTime.UtcNow;
        [Column("LIVI_LeaveAt")]
        public DateTime? LeaveAt { get; set; }
        [Column("LIVI_IsConnected")]
        public bool IsConnected { get; set; } = true;
        public Live Live { get; set; } = null!;
    }
}
