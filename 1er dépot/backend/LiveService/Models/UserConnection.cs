using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tweetz.MicroServices.LiveService.Models
{
    public class UserConnection
    {
        public string Username { get; set; } = string.Empty;
        public string LiveRoom { get; set; } = string.Empty;
    }
}