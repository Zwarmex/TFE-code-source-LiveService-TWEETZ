using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetz.MicroServices.LiveService.Dtos.Live;

namespace Tweetz.MicroServices.LiveService.Mappers
{
    public static class LiveMappers
    {
        public static LiveDto ToLiveDto(this Live liveModel)
        {
            return new LiveDto
            {
                ApiVideoLiveStreamId = liveModel.ApiVideoLiveStreamId,
                IsPublic = liveModel.IsPublic,
                Title = liveModel.Title,
                Views = liveModel.Views,
                StreamerId = liveModel.StreamerId,
                StreamerUsername = liveModel.StreamerUsername,
                StartTime = liveModel.StartTime,
                EndTime = liveModel.EndTime,
                PlayerUrl = liveModel.PlayerUrl,
                Broadcasting = liveModel.Broadcasting,
                ThumbnailUrl = liveModel.ThumbnailUrl,
                ViewCount = liveModel.ViewCount
            };
        }
        public static Live ToLiveModel(this JToken jToken)
        {
            return new Live
            {
                ApiVideoLiveStreamId = jToken["liveStreamId"].ToString(),
                Title = jToken["name"].ToString(),
                CreatedAt = DateTime.Parse(jToken["createdAt"].ToString()),
                UpdatedAt = DateTime.Parse(jToken["updatedAt"].ToString()),
                IsPublic = jToken["public"].ToObject<bool>(),
                Broadcasting = jToken["broadcasting"].ToObject<bool>(),
                PlayerUrl = jToken["assets"]["player"]?.ToString(),
                ThumbnailUrl = jToken["assets"]["thumbnail"]?.ToString()
                // Other properties are not available from API.Video and would be populated elsewhere
            };
        }
    }
}