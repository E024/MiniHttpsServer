using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MiniHttpsServer
{
    internal class HttpResponse : BaseHeader
    {
        private List<string> cookie = new List<string>();
        internal string StatusCode { get; set; }

        internal string Protocols { get; set; }

        internal string ProtocolsVersion { get; set; }

        internal byte[] Content { get; private set; }

        private Stream handler;


        internal HttpResponse(Stream stream)
        {
            this.handler = stream;
            this.Headers = new Dictionary<string, string>();
        }

        internal HttpResponse SetContent(byte[] content, Encoding encoding = null)
        {
            this.Content = content;
            this.Encoding = encoding != null ? encoding : Encoding.UTF8;
            this.Content_Length = content.Length.ToString();
            return this;
        }

        internal HttpResponse SetContent(string content, Encoding encoding = null)
        {
            //初始化内容
            encoding = encoding != null ? encoding : Encoding.UTF8;
            return SetContent(encoding.GetBytes(content), encoding);
        }

        internal Stream GetResponseStream()
        {
            return this.handler;
        }

        internal string GetHeader(ResponseHeaders header)
        {
            return GetHeaderByKey(header);
        }

        internal string GetHeader(string fieldName)
        {
            return GetHeaderByKey(fieldName);
        }

        internal void SetHeader(ResponseHeaders header, string value)
        {
            var fieldName = HeadersHelper.GetDescription(header);
            if (fieldName.Equals("Set-Cookie"))
            {
                cookie.Add(value);
                return;
            }
            SetHeaderByKey(header, value);
        }

        internal void SetHeader(string fieldName, string value)
        {
            if (fieldName.Equals("Set-Cookie"))
            {
                cookie.Add(value);
                return;
            }
            SetHeaderByKey(fieldName, value);
        }

        /// <summary>
        /// 构建响应头部
        /// </summary>
        /// <returns></returns>
        protected string BuildHeader()
        {

            StringBuilder builder = new StringBuilder();

            if (!string.IsNullOrEmpty(StatusCode))
                builder.Append("HTTP/1.1 " + StatusCode + "\r\n");

            if (!string.IsNullOrEmpty(this.Content_Type))
                builder.AppendLine("Content-Type:" + this.Content_Type);
            if (cookie.Any())
            {
                cookie.ForEach(item =>
                {
                    builder.AppendLine("Set-Cookie:" + item);
                });
            }
            if (Headers != null && Headers.Any())
            {
                foreach (var item in Headers)
                {
                    builder.AppendLine(item.Key + ":" + item.Value);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        internal void Send()
        {
            if (!handler.CanWrite) return;

            try
            {
                //发送响应头
                var header = BuildHeader();
                byte[] headerBytes = this.Encoding.GetBytes(header);
                handler.Write(headerBytes, 0, headerBytes.Length);

                //发送空行
                byte[] lineBytes = this.Encoding.GetBytes(System.Environment.NewLine);
                handler.Write(lineBytes, 0, lineBytes.Length);

                //发送内容
                handler.Write(Content, 0, Content.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //Log(e.Message);
            }
            finally
            {
                handler.Close();
                handler.Dispose();
            }
        }
    }
}
