using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MiniHttpsServer
{
    public enum Protocol
    {
        HTTPS,
        HTTP
    }
    public class TCPServer : IDisposable
    {
        protected readonly int port;

        protected readonly Protocol protocol;

        private TcpListener tcpListener;

        protected Thread listenerThread;
        private readonly List<TCPConnection> connections = new List<TCPConnection>();
        private readonly Type connectionType;
        internal int Port
        {
            get
            {
                return port;
            }
        }
        internal TCPServer(Protocol protocol, int port, Type connectionType)
        {
            this.protocol = protocol;
            this.port = port;
            this.connectionType = connectionType;
            Console.WriteLine("TCPServer:{0}", port);
        }
        internal void Start()
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            Console.WriteLine("监听端口{0}", port);
            listenerThread = new Thread(Listener);
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }
        protected virtual void OnStartListener()
        {
        }

        protected virtual void OnStopListener()
        {
        }

        protected void Listener()
        {
            Console.WriteLine("开始监听端口{0}...", port);
            OnStartListener();
            try
            {
                while (tcpListener != null)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    TCPConnection tCPConnection = (TCPConnection)Activator.CreateInstance(connectionType, new object[1] { protocol == Protocol.HTTPS });
                    tCPConnection.Start(this, tcpClient);
                    lock (connections)
                    {
                        connections.Add(tCPConnection);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(MethodBase.GetCurrentMethod() + "报错:" + ex);
            }
            finally
            {
                OnStopListener();
                int count;
                while ((count = connections.Count) > 0)
                {
                    try
                    {
                        TCPConnection tCPConnection2 = connections[count - 1];
                        tCPConnection2.Close();
                        tCPConnection2.Dispose();
                        if (connections.Count >= count)
                        {
                            connections.RemoveAt(count - 1);
                        }
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine(MethodBase.GetCurrentMethod() + "报错:" + ex2);
                    }
                }
            }
        }
        public void Dispose()
        {
            Close();
        }

        internal void Close()
        {
            if (tcpListener != null)
            {
                tcpListener.Stop();
                tcpListener = null;
            }
            if (listenerThread != null)
            {
                listenerThread.Abort();
                listenerThread = null;
            }
        }
    }
}
