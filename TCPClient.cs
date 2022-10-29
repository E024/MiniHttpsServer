using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace MiniHttpsServer
{
    public class TCPClient : IDisposable
    {
        protected readonly string host;
        protected readonly int port;
        protected readonly TCPConnection connection;
        public string Host
        {
            get
            {
                return host;
            }
        }
        public int Port
        {
            get
            {
                return port;
            }
        }

        public TCPClient(string host, int port, TCPConnection connection)
        {
            this.host = host;
            this.port = port;
            this.connection = connection;
            Console.Out.Write("TCPClient ; {0}:{1}\n", host, port);
        }

        public void Start()
        {
            TcpClient tcpClient = new TcpClient();
            IPAddress[] hostAddresses = Dns.GetHostAddresses(host);
            if (hostAddresses != null && hostAddresses.Length != 0)
            {
                tcpClient.Connect(hostAddresses[0], port);
                if (tcpClient.Connected)
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                    connection.Start(this, tcpClient);
                }
                return;
            }
            throw new Exception("无效IP地址 " + host);
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (connection != null)
            {
                connection.Close();
            }
        }

        public bool IsClosed()
        {
            return connection.IsClosed();
        }
    }
}
