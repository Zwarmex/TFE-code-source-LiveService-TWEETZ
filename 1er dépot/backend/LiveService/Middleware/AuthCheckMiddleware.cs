namespace Tweetz.MicroServices.LiveService.Middlewares;

public class AuthCheckMiddleware(RequestDelegate next, List<string> excludedPaths, IConfiguration configuration, ICryptService cryptService)
{
    private readonly RequestDelegate _next = next;
    private readonly List<string> _excludedPaths = excludedPaths;
    private readonly IConfiguration _configuration = configuration;
    private readonly ICryptService _cryptService = cryptService;

    public async Task Invoke(HttpContext context)
    {
        Services.RestClient.InitializeContext(context);

        //Excluded paths.
        if (_excludedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
        {
            string? cookie = context.Request.Cookies[_configuration.GetSection("AppSettings:CookieAccessName").Value!];

            if (cookie != null)
            {
                string? token = cookie.Split(".JTW")[0];
                context.Request.Headers.Append("Authorization", "Bearer " + token);
            }

            await _next(context);
            return;
        }

        //Admin cookie check.
        else if (context.Request.Cookies[GetAdminCookieName()] != null)
        {
            string cookie = context.Request.Cookies[GetAdminCookieName()]!;

            string[] cookieParts = cookie.Split(".JTW");
            string jwt = cookieParts[0];
            string signature = cookieParts[1];

            if (cookieParts.Length != 2)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Missing access cookie");
                return;
            }

            bool checkSingleCookie = await _cryptService.VerifyCookie(jwt, signature);

            if (!checkSingleCookie)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Missing access cookie");
                return;
            }

            context.Request.Headers.Append("Authorization", "Bearer " + jwt);
            await _next(context);
            return;
        }

        else
        {
            if (!await CookieChecker(context) || !JwtChecker(context))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Missing access cookie");
                return;
            }

            await _next(context); //continue.
        }
    }

    /// <summary>
    /// Checking on cookies.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<bool> CookieChecker(HttpContext context)
    {
        return await Task.Run(async () =>
        {
            try
            {
                List<string?> cookies = [context.Request.Cookies[_configuration.GetSection("AppSettings:CookieAccessName").Value!]!, context.Request.Cookies[_configuration.GetSection("AppSettings:CookieSessionName").Value!]!];
                bool checkResponse = await VerifyCookies(cookies);
                return checkResponse;
            }
            catch (Exception)
            {
                return false;
            }
        });
    }

    /// <summary>
    /// verifying a single cookie (signature)
    /// </summary>
    /// <param name="cookie"></param>
    /// <returns></returns>
    public async Task<bool> VerifyCookies(List<string?> cookies)
    {
        return await Task.Run(async () =>
        {
            foreach (string? cookie in cookies)
            {
                if (string.IsNullOrEmpty(cookie))
                {
                    return false;
                }
                else
                {
                    try
                    {
                        string[] cookieParts = cookie.Split(".JTW");
                        if (!await _cryptService.VerifyCookie(cookieParts[0], cookieParts[1]))
                        {
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            return true;
        });
    }

    /// <summary>
    /// base checking on JWT's
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private bool JwtChecker(HttpContext context)
    {
        try
        {
            string? cookie = context.Request.Cookies[_configuration.GetSection("AppSettings:CookieAccessName").Value!];

            if (string.IsNullOrEmpty(cookie))
            {
                return false;
            }

            string[] cookieParts = cookie.Split(".JTW");

            if (cookieParts.Length != 2)
            {
                return false;
            }

            string token = cookieParts[0] ?? throw new Exception("empty access token");

            context.Request.Headers.Append("Authorization", "Bearer " + token);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Retrieve access cookie from settings.
    /// </summary>
    /// <returns></returns>
    private string GetAdminCookieName()
    {
        return _configuration.GetSection("AppSettings:CookieAdminName").Value!;
    }
}
