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
            clientSocket.Send(Encoding.UTF8.GetBytes("hellow from client"));

        }
    }
    class Program
    {

        static void Main(string[] args)
        {
            var chatter = new Chatter("neozeo75", "Patrick Shim");
            var client = new Client();
            client.Connect("127.0.0.1", 56675);

        }
    }
}
