namespace Tweetz.MicroServices.LiveService.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        public DbSet<Live> Lives { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ModerationLog> ModerationLogs { get; set; }
        public DbSet<LiveViewer> LiveViewers { get; set; }
        public DbSet<LiveStatistic> LiveStatistics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional model configuration can be done here
        }
    }
}