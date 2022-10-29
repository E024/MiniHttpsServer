using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace MiniHttpsServer
{
    public class TCPConnection : IDisposable
    {
        protected TcpClient tcpClient;

        private readonly Certificate certificate;

        private readonly AsymmetricKeyEntry keyEntry;

        private static readonly SecureRandom secureRandom = new SecureRandom();

        protected readonly bool isSSL = true;

        protected BinaryReader inputStream;

        protected BinaryWriter outputStream;

        protected Thread runThread;

        protected TCPClient client;

        protected TCPServer server;

        public TCPConnection(bool isSSL)
        {
            this.isSSL = isSSL;
            if (!isSSL)
            {
                return;
            }
            string p12Passwd = "123456";
            AsymmetricKeyEntry asymmetricKeyEntry_;
            certificate = TlsUtils.GetCertificateFromP12("证书路径.pfx", p12Passwd, out asymmetricKeyEntry_);
            keyEntry = asymmetricKeyEntry_;
        }

        internal void Start(TCPClient client, TcpClient tcpClient)
        {
            this.client = client;
            Start(tcpClient);
        }

        internal void Start(TCPServer server, TcpClient tcpClient)
        {
            this.server = server;
            Start(tcpClient);
        }

        private void Start(TcpClient tcpClient)
        {
            Close();
            this.tcpClient = tcpClient;
            NetworkStream stream = tcpClient.GetStream();
            inputStream = new BinaryReader(stream);
            outputStream = new BinaryWriter(stream);
            if (isSSL)
            {
                if (server == null)
                {
                    try
                    {
                        TlsClient swTlsClient = new TlsClient(null, certificate, keyEntry);
                        TlsClientProtocol val = new TlsClientProtocol(stream, secureRandom);
                        val.Connect((Org.BouncyCastle.Crypto.Tls.TlsClient)(object)swTlsClient);
                        inputStream = new BinaryReader(val.Stream);
                        outputStream = new BinaryWriter(val.Stream);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(MethodBase.GetCurrentMethod() + "Client本次创建SslStream报错:\r\n", ex);
                    }
                }
                else
                {
                    try
                    {
                        TlsServer swTlsServer = new TlsServer(certificate, keyEntry);
                        TlsServerProtocol val2 = new TlsServerProtocol(stream, secureRandom);
                        val2.Accept((Org.BouncyCastle.Crypto.Tls.TlsServer)(object)swTlsServer);
                        inputStream = new BinaryReader(val2.Stream);
                        outputStream = new BinaryWriter(val2.Stream);

                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine(MethodBase.GetCurrentMethod() + "Server本次创建SslStream报错:\r\n", ex2);
                    }
                }
            }
            runThread = new Thread(Run);
            runThread.IsBackground = true;
            runThread.SetApartmentState(ApartmentState.STA);
            runThread.Start();
        }

        protected void Send(byte[] data)
        {
            try
            {
                outputStream.Write(data);
                outputStream.Flush();
            }
            catch
            {
            }
        }

        protected string ReadLine()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                try
                {
                    while (true)
                    {
                        int num = inputStream.Read();
                        if (num == -1 || num == 0 || num == 10)
                        {
                            break;
                        }
                        memoryStream.WriteByte((byte)num);
                    }
                    byte[] bytes = memoryStream.ToArray();
                    return Encoding.UTF8.GetString(bytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(MethodBase.GetCurrentMethod() + "接口传入的数据读取错误,可能是加密协议不符! 日志中见到这个错误请无视即可" + ex);
                    return string.Empty;
                }
            }
        }
        void HandAllowOrigin(HttpRequest request, HttpResponse response)
        {
            var referer = string.Empty;
            if (!request.Headers.TryGetValue("Referer", out referer))
            {
                return;
            }
            if (string.IsNullOrEmpty(referer))
            {
                return;
            }
            Uri uri = null;
            if (!Uri.TryCreate(referer, UriKind.RelativeOrAbsolute, out uri))
            {
                return;
            }
            if (uri == null)
            {
                return;
            }
            if (uri.Host.Equals("local.xxx.cn", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            if (uri.Host.EndsWith("xxx.com", StringComparison.CurrentCultureIgnoreCase))
            {
                response.SetHeader("Access-Control-Allow-Origin", uri.Host);
                response.SetHeader("Access-Control-Allow-Credentials", "true");
                response.SetHeader("Access-Control-Allow-Methods", "GET,POST");
            }
        }


        void OnGet(HttpRequest request, HttpResponse response)
        {
            HandAllowOrigin(request, response);
            if (request.URL.StartsWith("/xxx", StringComparison.CurrentCultureIgnoreCase))
            {
               
            }
            response.SetContent("<html><body><h1>404 - Not Found</h1></body></html>");
            response.StatusCode = "404";
            response.Content_Type = "text/html";
            //发送HTTP响应
            response.Send();

        }
        protected virtual void Run()
        {
            while (true)
            {
                try
                {

                    HttpRequest request = new HttpRequest(inputStream.BaseStream);
                    //request.Logger = Logger;

                    //构造HTTP响应
                    HttpResponse response = new HttpResponse(outputStream.BaseStream);
                    //response.Logger = Logger;
                    switch (request.Method)
                    {
                        case "GET":
                            OnGet(request, response);
                            break;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(MethodBase.GetCurrentMethod() + "接口传入的数据读取错误,可能是加密协议不符! 日志中见到这个错误请无视即可" + ex);
                    break;
                }
            }
            Close();
        }

        public void Dispose()
        {
            Close();
        }

        protected virtual void CloseConnection()
        {
            if (tcpClient == null)
            {
                return;
            }
            if (inputStream != null)
            {
                try
                {
                    inputStream.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(MethodBase.GetCurrentMethod() + "报错:" + ex);
                }
            }
            if (outputStream != null)
            {
                try
                {
                    outputStream.Close();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(MethodBase.GetCurrentMethod() + "报错:" + ex2);
                }
            }
            try
            {
                tcpClient.Close();
            }
            catch (Exception ex3)
            {
                Console.WriteLine(MethodBase.GetCurrentMethod() + "报错:" + ex3);
            }
            tcpClient = null;
            inputStream = null;
            outputStream = null;
        }

        internal bool IsClosed()
        {
            if (tcpClient != null)
            {
                return !tcpClient.Connected;
            }
            return true;
        }

        internal virtual void Close()
        {
            try
            {
                if (runThread != null)
                {
                    try
                    {
                        runThread.Abort();
                    }
                    catch
                    {
                    }
                    runThread = null;
                }
                CloseConnection();
            }
            finally
            {
                if (server != null)
                {
                }
            }
        }

    }
}
