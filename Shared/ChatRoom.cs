using System.Collections.Generic;

namespace Shared
{
    class ChatRoom
    {
        public ChatRoom()
        {
            ChatRoomId++;
            Chatters = new List<Chatter>();
        }

        public void EnterRoom(Chatter chatter)
        {
            Chatters.Add(chatter);
        }

        public void ExitRoom(Chatter chatter)
        {
            Chatters.Remove(chatter);
        }

        public Location Location { get; set; }
        public static int ChatRoomId { get; private set; }
        public List<Chatter> Chatters { get; set; }
        public int NumberOfChatters() => Chatters.Count;

    }
}
