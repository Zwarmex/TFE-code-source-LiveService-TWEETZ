namespace Tweetz.MicroServices.LiveService.Models
{
    public class ChatMessage
    {
        [Key]
        [Column("CHAT_MessageId")]
        public int MessageId { get; set; }

        [Required]
        [Column("CHAT_Content")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Column("CHAT_SenderId")]
        public int SenderId { get; set; }

        [Required]
        [Column("CHAT_Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [ForeignKey("Live")]
        [Column("CHAT_LIVE_LiveId")]
        public int LiveId { get; set; }

        // Navigation property
        [ForeignKey("LiveId")]
        public required Live Live { get; set; }
    }
}