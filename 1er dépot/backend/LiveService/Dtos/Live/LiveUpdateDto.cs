using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tweetz.MicroServices.LiveService.Dtos.Live
{
    public class LiveUpdateDto
    {
        public bool IsPublic { get; set; }
        public string Title { get; set; } = string.Empty;
        public int StreamerId { get; set; }
        public string PlayerUrl { get; set; } = string.Empty;
        public bool Broadcasting { get; set; } = false;
        public string ThumbnailUrl { get; set; } = string.Empty;
    }
}