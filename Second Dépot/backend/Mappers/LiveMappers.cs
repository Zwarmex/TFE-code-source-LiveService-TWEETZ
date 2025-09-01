namespace Tweetz.MicroServices.LiveService.Mappers
{
    public static class LiveMappers
    {
        public static LiveDto ToLiveDto(this Live liveModel)
        {
            return new LiveDto
            {
                ApiVideoLiveStreamId = liveModel.ApiVideoLiveStreamId,
                StreamKey = liveModel.ApiVideoStreamKey ?? string.Empty,
                IsPublic = liveModel.IsPublic,
                Title = liveModel.Title,
                StreamerId = liveModel.StreamerId,
                StreamerUsername = liveModel.StreamerUsername,
                StartTime = liveModel.StartTime,
                EndTime = liveModel.EndTime,
                Broadcasting = liveModel.Broadcasting ?? false,
                PlayerUrl = liveModel.PlayerUrl ?? string.Empty,
                ThumbnailUrl = liveModel.ThumbnailUrl ?? string.Empty,
                InvitedUserId = liveModel.InvitedUserId ?? null
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
            };
        }

        public static LiveUpdateDto ToLiveUpdateDto(this Live liveModel)
        {
            return new LiveUpdateDto
            {
                Title = liveModel.Title,
                IsPublic = liveModel.IsPublic,
            };
        }
    }
}