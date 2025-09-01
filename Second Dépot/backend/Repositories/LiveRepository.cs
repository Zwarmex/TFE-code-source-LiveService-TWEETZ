namespace Tweetz.MicroServices.LiveService.Repositories
{
    public class LiveRepository : ILiveRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LiveRepository> _logger;

        public LiveRepository(ApplicationDBContext context, IConfiguration configuration, ILogger<LiveRepository> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task CompleteLiveStreamAsync(string apiVideoLiveStreamId)
        {
            var basePath = _configuration["ApiVideo:ApiUrl"];
            var apiKey = _configuration["ApiVideo:ApiKey"];

            var client = new RestSharp.RestClient(new RestClientOptions(basePath));
            var request = new RestRequest($"live-streams/{apiVideoLiveStreamId}/complete", Method.Put);
            request.AddHeader("Authorization", $"Bearer {apiKey}");

            var live = await _context.Lives.FirstOrDefaultAsync(l => l.ApiVideoLiveStreamId == apiVideoLiveStreamId);
            if (live == null)
            {
                _logger.LogWarning("Live with ID {LiveId} not found.", apiVideoLiveStreamId);
                return;
            }

            var now = DateTime.UtcNow; // pour un timestamp cohérent
            await _context.LiveViewers
                .Where(v => v.LiveId == apiVideoLiveStreamId && v.IsConnected && v.LeaveAt == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(v => v.IsConnected, v => false)
                    .SetProperty(v => v.LeaveAt, v => now));

            await client.ExecuteAsync(request);

            await using var transaction = await _context.Database.BeginTransactionAsync();


            try
            {


                var uniqueViewers = await _context.LiveViewers
                    .Where(v => v.LiveId == apiVideoLiveStreamId)
                    .Select(v => v.UserId)
                    .Distinct()
                    .CountAsync();

                var uniqueChatters = await _context.ChatMessages
                    .Where(m => m.LiveId == apiVideoLiveStreamId)
                    .Select(m => m.SenderId)
                    .Distinct()
                    .CountAsync();


                var sessions = await _context.LiveViewers
                    .Where(v => v.LiveId == apiVideoLiveStreamId)
                    .Select(v => new { v.JoinAt, v.LeaveAt })
                    .ToListAsync();

                // Construction des événements
                var events = new List<(DateTime t, int delta, int ord)>();
                foreach (var s in sessions)
                {
                    // entrée
                    events.Add((s.JoinAt, +1, 1));
                    // sortie (si null, on utilise la fin du live)
                    var leave = s.LeaveAt ?? DateTime.UtcNow;
                    events.Add((leave, -1, 0)); // ord=0 pour traiter les sorties avant les entrées à t égal
                }

                // Tri: par temps puis par ord (sorties avant entrées)
                events.Sort((a, b) =>
                {
                    var cmp = a.t.CompareTo(b.t);
                    return cmp != 0 ? cmp : a.ord.CompareTo(b.ord);
                });

                // Balayage
                int current = 0, maxViewers = 0;
                foreach (var ev in events)
                {
                    current += ev.delta;
                    if (current > maxViewers)
                        maxViewers = current;
                }


                var startTime = live.StartTime;
                var endTime = DateTime.UtcNow;


                var watchTimes = await _context.LiveViewers
                    .Where(v => v.LiveId == apiVideoLiveStreamId && v.LeaveAt != null)
                    .Select(v => (v.LeaveAt.Value - v.JoinAt).TotalSeconds)
                    .ToListAsync();


                var avgWatchTime = watchTimes.Any() ? watchTimes.Average() : 0;


                var totalMessages = await _context.ChatMessages
                    .Where(m => m.LiveId == apiVideoLiveStreamId && !m.IsDeleted)
                    .CountAsync();

                var messagesByUser = await _context.ChatMessages
                    .Where(m => m.LiveId == apiVideoLiveStreamId && !m.IsDeleted)
                    .GroupBy(m => m.SenderUsername)
                    .Select(g => new
                    {
                        Username = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();




                double liveDuration = 0;

                liveDuration = (endTime - startTime.Value).TotalSeconds;


                // Retour JSON
                var statistic = new LiveStatistic
                {
                    ApiVideoLiveStreamId = apiVideoLiveStreamId,
                    LiveTitle = live.Title,
                    UserId = live.StreamerId,
                    IsPublic = live.IsPublic,
                    UniqueViewers = uniqueViewers,
                    TotalMessages = totalMessages,
                    MaxViewers = maxViewers,
                    UniqueChatters = uniqueChatters,
                    StartTime = startTime ?? DateTime.UtcNow,
                    EndTime = endTime,
                    TotalDuration = TimeSpan.FromSeconds(liveDuration),
                    AvgWatchDuration = TimeSpan.FromSeconds(avgWatchTime),
                    MessagesPerUserJson = System.Text.Json.JsonSerializer.Serialize(messagesByUser.ToDictionary(x => x.Username, x => x.Count)),
                };


                await _context.LiveStatistics.AddAsync(statistic);

                var deletedRowsLiveViewers = await _context.LiveViewers
                    .Where(v => v.LiveId == apiVideoLiveStreamId)
                    .ToListAsync();

                _context.LiveViewers.RemoveRange(deletedRowsLiveViewers);

                var deletedRowsMessages = await _context.ChatMessages
                    .Where(m => m.LiveId == apiVideoLiveStreamId)
                    .ToListAsync();

                _context.ChatMessages.RemoveRange(deletedRowsMessages);

                var deletedRowsLives = await _context.Lives
                    .Where(v => v.ApiVideoLiveStreamId == apiVideoLiveStreamId)
                    .ToListAsync();

                _context.Lives.RemoveRange(deletedRowsLives);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();


            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while getting live statistic for ID {LiveId}.", apiVideoLiveStreamId);
            }
        }

        public async Task<string> CreateAsync(LiveCreateDto live, int streamerId, string streamerUsername)
        {
            try
            {
                var apiKey = _configuration["ApiVideo:ApiKey"];
                var apiVideoClient = new ApiVideoClient(apiKey);

                var liveStreamCreationPayload = new LiveStreamCreationPayload()
                {
                    name = live.Title,
                    _public = live.IsPublic,
                };

                var liveStream = await apiVideoClient.LiveStreams().createAsync(liveStreamCreationPayload);
                if (liveStream != null && !string.IsNullOrEmpty(liveStream.livestreamid))
                {
                    var streamerLive = new Live
                    {
                        StreamerId = streamerId,
                        StreamerUsername = streamerUsername,
                        Title = live.Title,
                        ThumbnailUrl = liveStream.assets?.thumbnail,
                        IsPublic = live.IsPublic,
                        ApiVideoLiveStreamId = liveStream.livestreamid,
                        CreatedAt = DateTime.UtcNow,
                        ApiVideoStreamKey = liveStream.streamkey,
                        StartTime = live.StartTime,
                        EndTime = live.EndTime,
                        InvitedUserId = live.InvitedUserId
                    };
                    _context.Add(streamerLive);
                    await _context.SaveChangesAsync();
                    return liveStream.livestreamid;
                }
                return liveStream!.livestreamid;
            }
            catch (ApiException ex)
            {
                _logger.LogError("Erreur lors de la création du live stream : {message}", ex.Message);
                throw new Exception("Une erreur est survenue lors de la création du live stream.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erreur inattendue : {message}", ex.Message);
                throw;
            }
        }
        public async Task DeleteAsync(string apiVideoLiveStreamId, int streamerId)
        {
            var basePath = _configuration["ApiVideo:ApiUrl"];
            var apiKey = _configuration["ApiVideo:ApiKey"];

            // Remove from the DB
            var liveModel = await _context.Lives.FirstOrDefaultAsync(l => l.ApiVideoLiveStreamId == apiVideoLiveStreamId && l.StreamerId == streamerId);
            if (liveModel == null)
                throw new Exception("Live stream not found.");

            _context.Lives.Remove(liveModel);
            await _context.SaveChangesAsync();


            // If Success, it will remove from the API.Video

            var client = new RestSharp.RestClient(new RestClientOptions(basePath));
            var request = new RestRequest($"live-streams/{apiVideoLiveStreamId}", Method.Delete);
            request.AddHeader("Authorization", $"Bearer {apiKey}");

            await client.ExecuteAsync(request);
            return;
        }
        public async Task DeleteThumbnail(string apiVideoLiveStreamId, int streamerId)
        {
            var basePath = _configuration["ApiVideo:ApiUrl"];
            var apiKey = _configuration["ApiVideo:ApiKey"];

            var apiInstance = new ApiVideoClient(apiKey, basePath);
            var apiLiveStreamsInstance = apiInstance.LiveStreams();

            var liveModel = await _context.Lives.FirstOrDefaultAsync(l => l.ApiVideoLiveStreamId == apiVideoLiveStreamId && l.StreamerId == streamerId);
            if (liveModel == null)
                throw new Exception("Live stream not found.");

            try
            {
                LiveStream result = apiLiveStreamsInstance.deleteThumbnail(apiVideoLiveStreamId);
                return;
            }
            catch (ApiException ex)
            {
                _logger.LogError("Erreur lors de la suppression de la miniature : {message}", ex.Message);
                throw new Exception("Une erreur est survenue lors de la suppression de la miniature.", ex);
            }
        }
        public async Task<List<Live>> GetByStreamerIdAsync(int streamerId)
        {
            try
            {
                var lives = await _context.Lives
                    .Where(l => l.StreamerId == streamerId && l.IsTerminated == false)
                    .ToListAsync();

                return lives;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des lives pour le streamer {streamerId}", streamerId);
                return new List<Live>();
            }
        }
        public async Task<List<Live>> GetAllOnStreamAsync()
        {
            try
            {
                var livesOnStream = await _context.Lives
                    .Where(l => l.Broadcasting == true && l.IsPublic == true)
                    .ToListAsync();

                return livesOnStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des lives en cours de diffusion");
                return new List<Live>();
            }
        }
        public async Task<List<Live>> GetAllAsync()
        {
            try
            {
                var basePath = _configuration["ApiVideo:ApiUrl"];
                var apiKey = _configuration["ApiVideo:ApiKey"];

                var client = new RestSharp.RestClient(new RestClientOptions(basePath));
                var request = new RestRequest("live-streams", Method.Get);
                request.AddHeader("Authorization", $"Bearer {apiKey}");
                request.AddParameter("sortBy", "createdAt");

                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                        throw new UnauthorizedAccessException("API key is invalid or missing.");
                    else
                        throw new Exception($"API request failed with status code {response.StatusCode}");
                }

                // Parse le JSON pour debug
                var json = JObject.Parse(response.Content);
                var data = json["data"];

                var liveList = new List<Live>();

                if (data != null)
                {
                    foreach (var live in data)
                    {
                        liveList.Add(live.ToLiveModel());
                    }
                }
                else
                {
                }


                // Retourne la liste des live où IsPublic est vrai et Broadcasting est vrai
                return liveList.Where(l => l.IsPublic == true && l.Broadcasting == true).ToList();
            }
            catch (Exception ex)
            {
                return new List<Live>(); // <- retourne une liste vide en cas d'erreur pour respecter le type
            }
        }
        public async Task<Live?> GetByIdAsync(string apiVideoLiveStreamId, int userId)
        {
            try
            {
                // Récupérer depuis Api.Video
                var basePath = _configuration["ApiVideo:ApiUrl"];
                var apiKey = _configuration["ApiVideo:ApiKey"];

                var client = new RestSharp.RestClient(new RestClientOptions(basePath));
                var request = new RestRequest($"live-streams/{apiVideoLiveStreamId}", Method.Get);
                request.AddHeader("Authorization", $"Bearer {apiKey}");

                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    _logger.LogError("Erreur API.Video : {status} - {content}", response.StatusCode, response.Content);

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        return null;

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                        throw new UnauthorizedAccessException("API key is invalid or missing.");
                    else
                        throw new Exception($"API request failed with status code {response.StatusCode}");
                }
                if (string.IsNullOrEmpty(response.Content))
                {
                    _logger.LogError("API response content is null or empty for live stream ID {id}", apiVideoLiveStreamId);
                    return null;
                }
                var apiVideoLiveData = JObject.Parse(response.Content);



                var liveData = await _context.Lives
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.ApiVideoLiveStreamId == apiVideoLiveStreamId);

                if (apiVideoLiveData["public"]?.Value<bool>() == false && (liveData?.InvitedUserId == userId || liveData?.StreamerId == userId))
                {
                    liveData.PlayerUrl = apiVideoLiveData["assets"]["player"]?.ToString();
                    liveData.ThumbnailUrl = apiVideoLiveData["assets"]["thumbnail"]?.ToString();
                }
                else if (apiVideoLiveData["public"]?.Value<bool>() == false)
                {
                    // Live privé, et l'utilisateur n'est pas autorisé
                    return liveData;
                }

                if (apiVideoLiveData["public"]?.Value<bool>() == true)
                {
                    liveData.PlayerUrl = apiVideoLiveData["assets"]["player"]?.ToString();
                    liveData.ThumbnailUrl = apiVideoLiveData["assets"]["thumbnail"]?.ToString();
                }


                return liveData;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du live {id}", apiVideoLiveStreamId);
                return null;
            }
        }
        public async Task UpdateAsync(string apiVideoLiveStreamId, LiveUpdateDto live, int streamerId)
        {
            var basePath = _configuration["ApiVideo:ApiUrl"];
            var apiKey = _configuration["ApiVideo:ApiKey"];
            var apiVideoClient = new ApiVideoClient(apiKey, basePath);
            var client = new RestSharp.RestClient(new RestClientOptions(basePath));
            var request = new RestRequest($"live-streams/{apiVideoLiveStreamId}", Method.Delete);
            request.AddHeader("Authorization", $"Bearer {apiKey}");



            var liveStreamUpdatePayload = new LiveStreamUpdatePayload()
            {
                name = live.Title,
                _public = live.IsPublic,
            };

            var apiLiveStreamsInstance = apiVideoClient.LiveStreams();
            try
            {
                LiveStream result = apiLiveStreamsInstance.update(apiVideoLiveStreamId, liveStreamUpdatePayload);

                if (result != null)
                {
                    var stockModel = _context.Lives.FirstOrDefault(l => l.ApiVideoLiveStreamId == apiVideoLiveStreamId && l.StreamerId == streamerId);
                    if (stockModel != null)
                    {
                        stockModel.Title = live.Title;
                        stockModel.IsPublic = live.IsPublic;
                        if (live.IsPublic == true && stockModel.InvitedUserId.HasValue)
                            stockModel.InvitedUserId = null; // Clear invited user if live is now public
                        if (live.IsPublic == false)
                            stockModel.InvitedUserId = live.InvitedUserId; // Update invited user if live is private
                        stockModel.Broadcasting = live.Broadcasting;
                        if (live.IsTerminated.Equals(true))
                        {
                            stockModel.IsTerminated = true;
                            await client.ExecuteAsync(request);
                        }
                        if (live.StartTime.HasValue)
                            stockModel.StartTime = live.StartTime;
                        if (live.EndTime.HasValue)
                            stockModel.EndTime = live.EndTime;
                        stockModel.UpdatedAt = DateTime.UtcNow;
                        _context.Lives.Update(stockModel);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                    }

                }

            }
            catch (Exception ex)
            {
            }


        }
        public async Task UploadThumbnailAsync(string apiVideoLiveStreamId, IFormFile thumbnail)
        {
            var basePath = _configuration["ApiVideo:ApiUrl"];
            var apiKey = _configuration["ApiVideo:ApiKey"];
            var client = new RestSharp.RestClient(new RestClientOptions(basePath));
            var check = new RestRequest($"live-streams/{apiVideoLiveStreamId}", Method.Get);
            check.AddHeader("Authorization", $"Bearer {apiKey}");
            var checkRes = await client.ExecuteAsync(check);
            _logger.LogInformation("GET live-streams/{id} -> {status} {content}", apiVideoLiveStreamId, checkRes.StatusCode, checkRes.Content);



            var request = new RestRequest($"live-streams/{apiVideoLiveStreamId}/thumbnail", Method.Post);

            request.AddHeader("Authorization", $"Bearer {apiKey}");
            request.AlwaysMultipartFormData = true;

            byte[] bytes;
            await using (var ms = new MemoryStream())
            {
                await thumbnail.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            request.AddFile("file", bytes, thumbnail.FileName, string.IsNullOrWhiteSpace(thumbnail.ContentType) ? "application/octet-stream" : thumbnail.ContentType);

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                _logger.LogError("Erreur api.video thumbnail: {status} - {content}", response.StatusCode, response.Content);
                throw new Exception($"API request failed with status code {response.StatusCode}");
            }

        }
        public async Task<int> GetViewerCountAsync(string apiVideoLiveStreamId)
        {
            var count = await _context.LiveViewers
                .Where(v => v.LiveId == apiVideoLiveStreamId && v.IsConnected)
                .CountAsync();

            return count;
        }

    }
}