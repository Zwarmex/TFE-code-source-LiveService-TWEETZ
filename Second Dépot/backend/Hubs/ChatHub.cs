namespace Tweetz.MicroServices.LiveService.Hubs
{

    public class ChatHub : Hub
    {
        private readonly ApplicationDBContext _context;

        // Cache local pour les bans / timeouts
        private static Dictionary<int, DateTime?> _bannedUsers = new();

        public ChatHub(ApplicationDBContext context)
        {
            _context = context;
        }



        public async Task SendMessage(string liveId, int senderId, string senderUsername, string content)
        {
            if (_bannedUsers.TryGetValue(senderId, out DateTime? until) && until.HasValue && until > DateTime.UtcNow)
            {
                await Clients.Caller.SendAsync("ReceiveSystemMessage", "Vous êtes temporairement suspendu du chat.");
                return;
            }

            // Sauvegarde en DB
            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                LiveId = liveId,
                SenderId = senderId,
                SenderUsername = senderUsername,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();


            await Clients.Group(liveId).SendAsync("ReceiveMessage", message);
        }

        public async Task JoinLive(string liveId, int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, liveId);
            // await Clients.Caller.SendAsync("ReceiveSystemMessage", $"Rejoint le live {liveId}");
            var viewer = _context.LiveViewers
                .FirstOrDefault(v => v.LiveId == liveId && v.UserId == userId && v.IsConnected);

            if (viewer == null)
            {
                _context.LiveViewers.Add(new LiveViewer
                {
                    LiveId = liveId,
                    UserId = userId,
                    JoinAt = DateTime.UtcNow,
                    IsConnected = true
                });
            }
            else
            {
                viewer.IsConnected = true;
            }

            await _context.SaveChangesAsync();
        }
        public async Task LeaveLive(string liveId, int userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, liveId);
            // await Clients.Caller.SendAsync("ReceiveSystemMessage", $"A quitté le live {liveId}");

            var viewer = _context.LiveViewers
                .FirstOrDefault(v => v.LiveId == liveId && v.UserId == userId && v.IsConnected);

            if (viewer != null)
            {
                viewer.IsConnected = false;
                viewer.LeaveAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteMessage(Guid messageId, int moderatorId, string liveId)
        {
            var msg = await _context.ChatMessages.FindAsync(messageId);
            if (msg != null)
            {
                msg.IsDeleted = true;
                await _context.SaveChangesAsync();

                // Log
                var log = new ModerationLog
                {
                    LiveId = liveId,
                    ModeratorId = moderatorId,
                    TargetUserId = msg.SenderId,
                    ActionType = "DeleteMessage",
                    ActionDate = DateTime.UtcNow
                };

                _context.ModerationLogs.Add(log);
                await _context.SaveChangesAsync();

                // Notifier les clients
                await Clients.Group(liveId).SendAsync("MessageDeleted", messageId);
            }
        }

        public async Task TimeoutUser(string liveId, int moderatorId, int targetUserId, string targetUsername, int durationSeconds)
        {
            _bannedUsers[targetUserId] = DateTime.UtcNow.AddSeconds(durationSeconds);
            int StreamerId = _context.Lives.FirstOrDefault(l => l.ApiVideoLiveStreamId == liveId).StreamerId;
            // Log
            var log = new ModerationLog
            {
                LiveId = liveId,
                ModeratorId = moderatorId,
                StreamerId = StreamerId,
                TargetUsername = targetUsername,
                TargetUserId = targetUserId,
                ActionType = "Timeout",
                DurationSeconds = durationSeconds,
                ActionDate = DateTime.UtcNow
            };

            _context.ModerationLogs.Add(log);
            await _context.SaveChangesAsync();

            await Clients.Group(liveId).SendAsync("UserTimeout", targetUserId, durationSeconds);
        }

        public async Task BanUser(string liveId, int moderatorId, int targetUserId, string targetUsername)
        {
            _bannedUsers[targetUserId] = DateTime.MaxValue;

            int StreamerId = _context.Lives.FirstOrDefault(l => l.ApiVideoLiveStreamId == liveId).StreamerId;

            // Log
            var log = new ModerationLog
            {
                LiveId = liveId,
                ModeratorId = moderatorId,
                StreamerId = StreamerId,
                TargetUserId = targetUserId,
                TargetUsername = targetUsername,
                ActionType = "Ban",
                ActionDate = DateTime.UtcNow
            };

            _context.ModerationLogs.Add(log);
            await _context.SaveChangesAsync();

            await Clients.Group(liveId).SendAsync("UserBanned", targetUserId);
        }
    }

}