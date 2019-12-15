using System.Net.Sockets;

namespace Messenger
{
    // Object class to hold connection properties throughout the application
    class State
    {
        public Socket Socket;
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
    }
}
