using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tweetz.MicroServices.LiveService.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinChat(UserConnection conn)
        {
            await Clients.All
                .SendAsync("ReceiveMessage", "ChatBOT", $"{conn.Username} has joined the live.");
        }
        public async Task JoinSpecificChatRoom(UserConnection conn)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conn.LiveRoom);
            await Clients.Group(conn.LiveRoom)
                .SendAsync("ReceiveMessage", "ChatBOT", $"{conn.Username} has joined the room.");
        }



    }
}