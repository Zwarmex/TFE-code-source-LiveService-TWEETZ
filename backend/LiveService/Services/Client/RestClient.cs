namespace Tweetz.MicroServices.LiveService.Services;

// LORS DE LIMPLEMENTATION DU RESTCLIENT
// LINITALISER DANS LE PROGRAM(POUR LA CONFIGURATION) et COOKIEMIDDLEWARE(POUR LE CONTEXT)
public static class RestClient
{
    private static readonly string _rootKey = "MicroServices";
    private static string _accessCookie = string.Empty;
    private static string _sessionCookie = string.Empty;
    private static string _baseUrl = string.Empty;
    private static int? _userId;
    private static string _username = string.Empty;
    private static TinyRestClient? _tinyClient = null;
    private static HttpClient? _httpClient;
    private static IConfiguration? _configuration;
    private static HttpContext? _context;

    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static void InitializeContext(HttpContext context)
    {
        _context = context;
    }

    #region MICROSERVICE LIST
    public static void InitializeChatService()
    {
        _baseUrl = _configuration?.GetSection($"{_rootKey}:ChatService").Get<string>() ?? "http://chatservice:80/api/v1/chat/";
    }
    public static void InitializeContentService()
    {
        _baseUrl = _configuration?.GetSection($"{_rootKey}:ContentService").Get<string>() ?? "http://contentservice:80/api/v1/content/";
    }
    public static void InitializePaymentService()
    {
        _baseUrl = _configuration?.GetSection($"{_rootKey}:PaymentService").Get<string>() ?? "http://payment:80/api/v1/payment/";
    }
    public static void InitializePublicationService()
    {
        _baseUrl = _configuration?.GetSection($"{_rootKey}:Publication").Get<string>() ?? "http://publication:80/api/v1/publication";
    }
    public static void InitializeUserService()
    {
        _baseUrl = _configuration?.GetSection($"{_rootKey}:UserService").Get<string>() ?? "http://userservice:80/api/v1/user/";
    }
    #endregion

    #region OLD WAY TO REFRESH TOKEN
    public static void InitializeUserIdAndUserName(int? userId, string username)
    {
        _userId = userId;
        _username = username;
    }
    #endregion

    public static TinyRestClient Instance
    {
        get
        {
            _tinyClient ??= InitClient();
            return _tinyClient;
        }
    }

    // Faire l'initialisation du client pour chaque service
    public static TinyRestClient InitClient()
    {
        try
        {
            _httpClient = new HttpClient();
            return new TinyRestClient(_httpClient, _baseUrl);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static void RefreshToken()
    {
        try
        {
            if (_configuration != null && _context != null && _tinyClient != null)
            {
                string cookieAccessName = _configuration?.GetValue<string>("AppSettings:CookieAccessName") ?? string.Empty;
                string cookieSessionName = _configuration?.GetValue<string>("AppSettings:CookieSessionName") ?? string.Empty;

                if (!string.IsNullOrEmpty(cookieAccessName) && !string.IsNullOrEmpty(cookieSessionName))
                {
                    _accessCookie = cookieAccessName + "=" + _context.Request.Cookies[cookieAccessName];
                    _sessionCookie = cookieSessionName + "=" + _context.Request.Cookies[cookieSessionName];
                    _tinyClient.Settings.DefaultHeaders.Add("Cookie", _accessCookie);
                    _tinyClient.Settings.DefaultHeaders.Add("Cookie", _sessionCookie);
                }
                else
                {
                    throw new ArgumentException("cookie not found in configuration");
                }
            }
            else
            {
                throw new ArgumentNullException("configuration null");
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static async Task<TResult> ExecuteAsyncWithToken<TResult>(this IParameterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Tenter de prendre le token qui vient de la request
            if (string.IsNullOrEmpty(_accessCookie) && string.IsNullOrEmpty(_sessionCookie))
            {
                RefreshToken();
            }

            return await request.ExecuteAsync<TResult>(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        }
        catch (Exception)
        {
            throw;
        }
    }

    public static async Task ExecuteAsyncWithToken(this IParameterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_accessCookie) && string.IsNullOrEmpty(_sessionCookie))
            {
                RefreshToken();
            }
            await request.ExecuteAsync(cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
