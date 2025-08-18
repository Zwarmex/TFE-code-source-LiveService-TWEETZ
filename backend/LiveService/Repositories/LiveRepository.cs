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

        public Task CompleteLiveStreamAsync(string apiVideoLiveStreamId)
        {
            var basePath = _configuration["ApiVideo:ApiUrl"];
            var apiKey = _configuration["ApiVideo:ApiKey"];

            var client = new RestSharp.RestClient(new RestClientOptions(basePath));
            var request = new RestRequest($"live-streams/{apiVideoLiveStreamId}/complete", Method.Put);
            request.AddHeader("Authorization", $"Bearer {apiKey}");

            return client.ExecuteAsync(request);
        }

        public async Task CreateAsync(Live live, string streamerId)
        {
            try
            {
                var apiKey = _configuration["ApiVideo:ApiKey"];
                var apiVideoClient = new ApiVideoClient(apiKey);

                // Préparer les données pour la création du live stream
                var liveStreamCreationPayload = new LiveStreamCreationPayload()
                {
                    name = live.Title,
                    _public = live.IsPublic,
                };

                // Créer le live stream
                var liveStream = await apiVideoClient.LiveStreams().createAsync(liveStreamCreationPayload);

                // Sauvegarder les informations dans la base de données
                if (liveStream != null && !string.IsNullOrEmpty(liveStream.livestreamid))
                {
                    var streamerLive = new StreamerLive
                    {
                        StreamerId = streamerId,
                        ApiVideoLiveStreamId = liveStream.livestreamid
                    };
                    _context.Add(streamerLive);
                    await _context.SaveChangesAsync();
                }
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

        // public async Task CreateAsync(Live live, string streamerId)
        // {
        //     var basePath = _configuration["ApiVideo:ApiUrl"];
        //     var apiKey = _configuration["ApiVideo:ApiKey"];

        //     var client = new RestSharp.RestClient(new RestClientOptions(basePath));
        //     var request = new RestRequest("live-streams", Method.Post);
        //     request.AddHeader("Authorization", $"Bearer {apiKey}");



        //     // Construct the payload with the required 'dto' field
        //     var payload = new
        //     {
        //         liveStreamCreationPayload = new
        //         {
        //             isPublic = live.IsPublic,
        //             title = live.Title,
        //             streamerId = streamerId,
        //             playerUrl = live.PlayerUrl,
        //             broadcasting = live.Broadcasting,
        //             thumbnailUrl = live.ThumbnailUrl
        //         }
        //     };

        //     request.AddJsonBody(payload);

        //     var response = await client.ExecuteAsync(request);

        //     if (!response.IsSuccessful)
        //     {
        //         _logger.LogError("Erreur API.Video : {status} - {content}", response.StatusCode, response.Content);
        //         throw new Exception($"API request failed with status code {response.StatusCode}");
        //     }

        //     var liveId = JObject.Parse(response.Content)["liveStreamId"]?.ToString();

        //     if (!string.IsNullOrEmpty(liveId))
        //     {
        //         var streamerLive = new StreamerLive
        //         {
        //             StreamerId = streamerId,
        //             ApiVideoLiveStreamId = liveId
        //         };
        //         _context.Add(streamerLive);
        //         await _context.SaveChangesAsync();
        //     }
        // }

        public Task DeleteAsync(string apiVideoLiveStreamId, string streamerId)
        {
            var basePath = _configuration["ApiVideo:ApiUrl"];
            var apiKey = _configuration["ApiVideo:ApiKey"];

            var client = new RestSharp.RestClient(new RestClientOptions(basePath));
            var request = new RestRequest($"live-streams/{apiVideoLiveStreamId}", Method.Delete);
            request.AddHeader("Authorization", $"Bearer {apiKey}");

            return client.ExecuteAsync(request);
        }

        public Task DeleteThumbnailAsync(string apiVideoLiveStreamId, string thumbnailUrl)
        {
            var basePath = _configuration["ApiVideo:ApiUrl"];
            var apiKey = _configuration["ApiVideo:ApiKey"];

            var client = new RestSharp.RestClient(new RestClientOptions(basePath));
            var request = new RestRequest($"live-streams/{apiVideoLiveStreamId}/thumbnail", Method.Delete);
            request.AddHeader("Authorization", $"Bearer {apiKey}");
            request.AddParameter("thumbnailUrl", thumbnailUrl);

            return client.ExecuteAsync(request);
        }

        // TO TEST : curl -H "Authorization: Bearer fg5iciCl24qhozuZJ3iLh2V36d6OoRrwgqDVtHolsE" https://sandbox.api.video/live-streams | jq




        public async Task<List<Live>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Début de la récupération des lives depuis API.Video");

                var basePath = _configuration["ApiVideo:ApiUrl"];
                var apiKey = _configuration["ApiVideo:ApiKey"];

                var client = new RestSharp.RestClient(new RestClientOptions(basePath));
                var request = new RestRequest("live-streams", Method.Get);
                request.AddHeader("Authorization", $"Bearer {apiKey}");
                request.AddParameter("sortBy", "createdAt");

                _logger.LogInformation("Request URL: {url}", client.BuildUri(request));
                _logger.LogInformation("Using API key: {key}", apiKey);

                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    _logger.LogError("Erreur API.Video : {status} - {content}", response.StatusCode, response.Content);
                    Console.WriteLine($"Erreur API.Video : {response.StatusCode}");

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                        throw new UnauthorizedAccessException("API key is invalid or missing.");
                    else
                        throw new Exception($"API request failed with status code {response.StatusCode}");
                }

                Console.WriteLine("JSON reçu de API.Video :");
                Console.WriteLine(response.Content);

                // Parse le JSON pour debug
                var json = JObject.Parse(response.Content);
                var data = json["data"];

                var liveList = new List<Live>();

                if (data != null)
                {
                    Console.WriteLine("\nListe des lives :");
                    foreach (var live in data)
                    {
                        Console.WriteLine($"- Id: {live["liveStreamId"]}, Name: {live["name"]}, Public: {live["public"]}, Broadcasting: {live["broadcasting"]}");


                        liveList.Add(live.ToLiveModel());
                    }
                }
                else
                {
                    Console.WriteLine("Aucun live trouvé dans la réponse.");
                }

                _logger.LogInformation("Récupération des lives terminée");

                // Retourne la liste
                return liveList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur non gérée dans GetAllAsync_Debug");
                Console.WriteLine($"Exception : {ex.Message}");
                return new List<Live>(); // <- retourne une liste vide en cas d'erreur pour respecter le type
            }
        }

        public async Task<Live?> GetByIdAsync(string apiVideoLiveStreamId)
        {
            try
            {
                _logger.LogInformation("Récupération du live {id} depuis API.Video", apiVideoLiveStreamId);

                var basePath = _configuration["ApiVideo:ApiUrl"];
                var apiKey = _configuration["ApiVideo:ApiKey"];

                var client = new RestSharp.RestClient(new RestClientOptions(basePath));
                var request = new RestRequest($"live-streams/{apiVideoLiveStreamId}", Method.Get);
                request.AddHeader("Authorization", $"Bearer {apiKey}");

                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    _logger.LogError("Erreur API.Video : {status} - {content}", response.StatusCode, response.Content);

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return null;

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        throw new UnauthorizedAccessException("API key is invalid or missing.");
                    else
                        throw new Exception($"API request failed with status code {response.StatusCode}");
                }

                // Parse the JSON response
                var liveData = JObject.Parse(response.Content);

                // Use the mapper to convert from JToken to Live model
                return liveData.ToLiveModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du live {id}", apiVideoLiveStreamId);
                return null;
            }
        }
        public Task UpdateAsync(string apiVideoLiveStreamId, LiveUpdateDto live, string streamerId)
        {
            var basePath = _configuration["ApiVideo:ApiUrl"];
            var apiKey = _configuration["ApiVideo:ApiKey"];

            var client = new RestSharp.RestClient(new RestClientOptions(basePath));
            var request = new RestRequest($"live-streams/{apiVideoLiveStreamId}", Method.Patch);
            request.AddHeader("Authorization", $"Bearer {apiKey}");
            request.AddJsonBody(live);

            return client.ExecuteAsync(request);
        }

        public Task UploadThumbnailAsync(string apiVideoLiveStreamId, string thumbnail)
        {
            var basePath = _configuration["ApiVideo:ApiUrl"];
            var apiKey = _configuration["ApiVideo:ApiKey"];

            var client = new RestSharp.RestClient(new RestClientOptions(basePath));
            var request = new RestRequest($"live-streams/{apiVideoLiveStreamId}/thumbnail", Method.Post);
            request.AddHeader("Authorization", $"Bearer {apiKey}");
            request.AddFile("thumbnail", thumbnail, Path.GetFileName(thumbnail));

            return client.ExecuteAsync(request);
        }

    }
}