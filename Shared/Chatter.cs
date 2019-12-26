using System;
using System.Net.Sockets;
using System.Text;

namespace Shared
{
    enum Location
    {
        LOBBY,
        CHATROOM
    }
    enum Control
    {
        ENTER,
        LEAVE,
        LOGGIN,
        LOGOUT,
    }

    enum CurrentState
    {
        CONNECTED,
        DISCONNECTED,
        AWAY
    }

    class Chatter
    {
        public Chatter() {
        }
        public Chatter(string chatterId, string chatterName)
        {
            ChatterId = chatterId;
            ChatterName = chatterName;
        }
        public string ChatterId { get; set; }
        public string ChatterName { get; set; }
        public Location Location { get; set; }
        public CurrentState CurrentState { get; set; }
        public Socket ChatterSocket { get; set; }
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
    }
}
