using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Threading;
using Shared;

namespace Client
{
    class Client
    {
        private Socket _clientSocket;

        public Client()
        {
           
        }

        public void Connect(string IpAddress, int port)
        {
            try
            {
                var endpoint = new IPEndPoint(IPAddress.Parse(IpAddress), port);
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.BeginConnect(endpoint, new AsyncCallback(OnConnected), _clientSocket);
            }
            catch(Exception e)
            {

            }
        }
        public void OnConnected(IAsyncResult ar)
        {
            var clientSocket = (Socket) ar.AsyncState;

        }
    }
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
