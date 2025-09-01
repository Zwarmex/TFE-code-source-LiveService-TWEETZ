

namespace Tweetz.MicroServices.LiveService.Utils
{
    public class Utilities
    {
        public static string? ExtractPrivateToken(LiveStream? liveStream, LiveCreateDto? live)
        {
            if (liveStream == null || string.IsNullOrEmpty(liveStream.livestreamid))
                return null;
            if (live?.IsPublic != false)
                return null; // uniquement si non public

            var url = liveStream.assets?.player;
            if (string.IsNullOrWhiteSpace(url))
                return null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return null;

            var query = QueryHelpers.ParseQuery(uri.Query);
            return query.TryGetValue("token", out var values) ? values.ToString() : null;
        }
    }
}