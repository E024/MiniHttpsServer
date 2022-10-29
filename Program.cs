using System;
using System.Collections.Generic;
using System.Text;

namespace MiniHttpsServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TCPServer server = new TCPServer(Protocol.HTTPS, 443, typeof(TCPConnection));
            server.Start();
        }
    }
}
