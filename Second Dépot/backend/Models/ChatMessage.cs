namespace Tweetz.MicroServices.LiveService.Models
{
    [Table("CHAT_MESSAGES")]
    public class ChatMessage
    {
        [Key]
        [Column("MSG_Id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("Live")]
        [Column("LIVE_ApiVideoLiveStreamId")]
        public string LiveId { get; set; } = string.Empty;

        [Required]
        [Column("MSG_SenderId")]
        public int SenderId { get; set; }

        [Required]
        [Column("MSG_SenderUsername")]
        public string SenderUsername { get; set; } = string.Empty;

        [Required]
        [Column("MSG_Content")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Column("MSG_SentAt")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Column("MSG_IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        public Live Live { get; set; } = null!;
    }
}
