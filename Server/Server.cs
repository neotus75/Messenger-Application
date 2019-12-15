using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics;

namespace Messenger
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

    class Chatter
    {
        public Chatter() { }
        public Location Location { get; set; }
        public bool IsConnected { get; set; }
    }

    class ChatRoom
    {
        
    }

    class Server {
        private Socket _serverSocket; 
        private List<Socket> _clientSocketList; 
        private ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        public Server()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _clientSocketList = new List<Socket>();
        }

        public void Start(int port, int backlog = 1000)
        {
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            _serverSocket.Listen(backlog);

            Console.WriteLine($"[{GetNumberConnected()}] Waiting for a connection...");

            while(true)
            {
              //  Socket.Select(_serverSocketList, null, null, 0);
                _manualResetEvent.Reset();
                _serverSocket.BeginAccept(new AsyncCallback(OnAccept), _serverSocket);
                _manualResetEvent.WaitOne();
            }
        }
        private int GetNumberConnected()
        {
            return (_clientSocketList != null) ? _clientSocketList.Count : 0;
        }
        private void OnAccept(IAsyncResult asyncResult)
        {
            _manualResetEvent.Set();

            var state = new State();
            var serverSocket = (Socket) asyncResult.AsyncState;
            var clientSocket = serverSocket.EndAccept(asyncResult);

            state.Socket = clientSocket;

            // greets the client at connection
            clientSocket.Send(Encoding.UTF8.GetBytes($"Greeting from SERVER [{serverSocket.LocalEndPoint}]\r\n"));

            // add client socket to the socket list as it gets connected.
            _clientSocketList.Add(clientSocket);

            // notify all the clients of new comer.
            Broadcast(clientSocket, $"[{GetNumberConnected()}] A client connected from {clientSocket.RemoteEndPoint.ToString()}\r\n");
            
            clientSocket.BeginReceive(state.Buffer, 
            0, 
            State.BufferSize, 
            SocketFlags.None, 
            new AsyncCallback(OnReceived), 
            state);
        }

        private void OnReceived(IAsyncResult asyncResult)
        {
            var stringbuilder = new StringBuilder();

            try
            {
                var state = (State) asyncResult.AsyncState;
                var clientSocket = state.Socket;
                var read = clientSocket.EndReceive(asyncResult);
                
                if (read > 0)
                {
                    stringbuilder.Append(Encoding.UTF8.GetString(state.Buffer, 0, read));

                    Broadcast(clientSocket, stringbuilder.ToString());    

                    clientSocket.BeginReceive(state.Buffer, 
                    0, 
                    State.BufferSize, 
                    SocketFlags.None, 
                    new AsyncCallback(OnReceived), 
                    state);
                }
                else 
                {
                    Broadcast(clientSocket, $"\r\n[{GetNumberConnected() - 1}] {clientSocket.RemoteEndPoint} left the room...\n\r");       
                    _clientSocketList.Remove(clientSocket);
                }
              
            } catch (SocketException e) {
                Console.WriteLine($"An error has occurred: {e.Message}");
            }
        }

        private void OnSent(IAsyncResult asyncResult)
        {
            var socket = (Socket)asyncResult.AsyncState;
            var sent = socket.EndSend(asyncResult);
            Trace.WriteLine($"{sent} byte(s) send to {socket.RemoteEndPoint.ToString()}");
        }

        private  void Broadcast(Socket currentSocket, string message)
        {
            try
            {
                foreach (var connectedSocket in _clientSocketList.Where(s => s.Connected == true))
                {
                    if (connectedSocket != currentSocket)
                    {
                        connectedSocket.BeginSend(Encoding.UTF8.GetBytes(message), 
                        0, 
                        message.Length, 
                        SocketFlags.None, 
                        new AsyncCallback(OnSent), 
                        connectedSocket);
                    }
                }
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending messages to clients (message: {ex.Message})");
            }
        }
    }
}
