using System;

namespace Messenger
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Start(56675);
            Console.WriteLine("Hello World!");
        }
    }
}
