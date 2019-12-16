using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics;
using Shared;

namespace Messenger
{
    class Server {
        private Socket _serverSocket; 
        private List<Socket> _clientSocketList; 
        private ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private List<ChatRoom> _chatRoomList;
        private ChatRoom _lobby;

        public Server()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _clientSocketList = new List<Socket>();
            _chatRoomList = new List<ChatRoom>();
            _lobby = new ChatRoom() { Location = Location.LOBBY, Chatters = new List<Chatter>() };
        }

        public void Start(int port, int backlog = 1000)
        {
            _chatRoomList.Add(_lobby);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            _serverSocket.Listen(backlog);
           
            Console.WriteLine($"[{_lobby.NumberOfChatters()}] Waiting for a connection...");

            while(true)
            {
              //  Socket.Select(_serverSocketList, null, null, 0);
                _manualResetEvent.Reset();
                _serverSocket.BeginAccept(new AsyncCallback(OnAccept), _serverSocket);
                _manualResetEvent.WaitOne();
            }
        }
        
        // user connection go to "LOBBY" at first connection
        private void OnAccept(IAsyncResult asyncResult)
        {
            _manualResetEvent.Set();

            var chatter = new Chatter();

            var serverSocket = (Socket) asyncResult.AsyncState;
            chatter.ChatterSocket = serverSocket.EndAccept(asyncResult);

            
            chatter.ChatterSocket.Send(Encoding.UTF8.GetBytes($"Greeting from SERVER [{serverSocket.LocalEndPoint}]\r\n"));

            _lobby.EnterRoom(chatter);

            Console.WriteLine($"A client connected from {chatter.ChatterSocket.RemoteEndPoint.ToString()}\r\n");
            Console.WriteLine($"[{_lobby.NumberOfChatters()}] Waiting for a connection...");

            // notify all the clients of new comer.
            Broadcast(chatter, $"A client connected from {chatter.ChatterSocket.RemoteEndPoint.ToString()}\r\n");
            try 
            {
                chatter.ChatterSocket.BeginReceive(chatter.Buffer, 
                0, 
                Chatter.BufferSize, 
                SocketFlags.None, 
                new AsyncCallback(OnReceived), 
                chatter);
            } 
            catch (Exception se)
            {
                if (_lobby.Chatters.Count != 0)
                {
                    chatter.ChatterSocket.Shutdown(SocketShutdown.Both);
                    _lobby.ExitRoom(chatter);

                }
                Console.WriteLine(se.Message);
            }
        }

        private void OnReceived(IAsyncResult asyncResult)
        {
            var stringbuilder = new StringBuilder();

            try
            {
                //var state = (State) asyncResult.AsyncState;
                var chatter = (Chatter)asyncResult.AsyncState;

                //var clientSocket = state.Socket;
                var chatterSocket = chatter.ChatterSocket;

                var read = chatterSocket.EndReceive(asyncResult);
                
                if (read > 0)
                {
                    stringbuilder.Append(Encoding.UTF8.GetString(chatter.Buffer, 0, read));

                    Broadcast(chatter, stringbuilder.ToString());

                    chatterSocket.BeginReceive(chatter.Buffer, 
                    0, 
                    Chatter.BufferSize, 
                    SocketFlags.None, 
                    new AsyncCallback(OnReceived), 
                    chatter);
                }
                else 
                {
                    Broadcast(chatter, $"{chatter.ChatterSocket.RemoteEndPoint} left the room...\n\r");
                    _lobby.ExitRoom(chatter);
                }
              
            } catch (SocketException e) {
                Console.WriteLine($"An error has occurred: {e.Message}");
            }
        }

        private void OnSent(IAsyncResult asyncResult)
        {
            var chatter = (Chatter)asyncResult.AsyncState;
            var chatterSocket = chatter.ChatterSocket;
            var sent = chatterSocket.EndSend(asyncResult);
            Trace.WriteLine($"{sent} byte(s) send to {chatterSocket.RemoteEndPoint.ToString()}");
        }

        private  void Broadcast(Chatter currentChatter, string message)
        {
            try
            {
                foreach (var room in _chatRoomList)
                {
                    foreach(var chatter in room.Chatters.Where(s => s.ChatterSocket.Connected == true))
                    {
                        if (chatter != currentChatter)
                        {
                            chatter.ChatterSocket.BeginSend(Encoding.UTF8.GetBytes(message),
                            0,
                            message.Length,
                            SocketFlags.None,
                            new AsyncCallback(OnSent),
                            chatter);
                        }
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
