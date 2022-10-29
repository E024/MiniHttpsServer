﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniHttpsServer
{
    /// <summary>
    /// HTTP请求定义
    /// </summary>
    internal class HttpRequest : BaseHeader
    {
        /// <summary>
        /// URL参数
        /// </summary>
        internal Dictionary<string, string> Params { get; private set; }

        /// <summary>
        /// HTTP请求方式
        /// </summary>
        internal string Method { get; private set; }

        /// <summary>
        /// HTTP(S)地址
        /// </summary>
        internal string URL { get; set; }

        /// <summary>
        /// HTTP协议版本
        /// </summary>
        internal string ProtocolVersion { get; set; }

        /// <summary>
        /// 定义缓冲区
        /// </summary>
        private const int MAX_SIZE = 1024 * 1024 * 2;
        private byte[] bytes = new byte[MAX_SIZE];


        private Stream handler;

        internal HttpRequest(Stream stream)
        {
            this.handler = stream;
            var data = GetRequestData(handler);
            var rows = Regex.Split(data, Environment.NewLine);

            //Request URL & Method & Version
            var first = Regex.Split(rows[0], @"(\s+)")
                .Where(e => e.Trim() != string.Empty)
                .ToArray();
            if (first.Length > 0) this.Method = first[0];
            if (first.Length > 1) this.URL = Uri.UnescapeDataString(first[1]);
            if (first.Length > 2) this.ProtocolVersion = first[2];

            //Request Headers
            this.Headers = GetRequestHeaders(rows);

            //Request Body
            Body = GetRequestBody(rows);
            var contentLength = GetHeader(RequestHeaders.ContentLength);
            if (int.TryParse(contentLength, out var length) && Body.Length != length)
            {
                do
                {
                    length = stream.Read(bytes, 0, MAX_SIZE - 1);
                    Body += Encoding.UTF8.GetString(bytes, 0, length);
                } while (Body.Length != length);
            }

            //Request "GET"
            if (this.Method == "GET")
            {
                var isUrlencoded = this.URL.Contains("?");
                if (isUrlencoded) this.Params = GetRequestParameters(URL.Split('?')[1]);
            }

            //Request "POST"
            if (this.Method == "POST")
            {
                var contentType = GetHeader(RequestHeaders.ContentType);
                var isUrlencoded = contentType == @"application/x-www-form-urlencoded";
                if (isUrlencoded) this.Params = GetRequestParameters(this.Body);
            }
        }

        internal Stream GetRequestStream()
        {
            return this.handler;
        }

        internal string GetHeader(RequestHeaders header)
        {
            return GetHeaderByKey(header);
        }

        internal string GetHeader(string fieldName)
        {
            return GetHeaderByKey(fieldName);
        }

        internal void SetHeader(RequestHeaders header, string value)
        {
            SetHeaderByKey(header, value);
        }

        internal void SetHeader(string fieldName, string value)
        {
            SetHeaderByKey(fieldName, value);
        }

        private string GetRequestData(Stream stream)
        {
            var length = 0;
            var data = string.Empty;

            try
            {
                do
                {
                    length = stream.Read(bytes, 0, MAX_SIZE - 1);
                    data += Encoding.UTF8.GetString(bytes, 0, length);
                } while (length > 0 && !data.Contains("\r\n\r\n"));

            }
            catch (Exception)
            {

            }
            return data;
        }

        private string GetRequestBody(IEnumerable<string> rows)
        {
            var target = rows.Select((v, i) => new { Value = v, Index = i }).FirstOrDefault(e => e.Value.Trim() == string.Empty);
            if (target == null) return null;
            var range = Enumerable.Range(target.Index + 1, rows.Count() - target.Index - 1);
            return string.Join(Environment.NewLine, range.Select(e => rows.ElementAt(e)).ToArray());
        }

        private Dictionary<string, string> GetRequestHeaders(IEnumerable<string> rows)
        {
            if (rows == null || rows.Count() <= 0) return null;
            var target = rows.Select((v, i) => new { Value = v, Index = i }).FirstOrDefault(e => e.Value.Trim() == string.Empty);
            var length = target == null ? rows.Count() - 1 : target.Index;
            if (length <= 1) return null;
            var range = Enumerable.Range(1, length - 1);
            return range.Select(e => rows.ElementAt(e)).ToDictionary(e => e.Split(':')[0], e => String.Join(":", e.Split(':').Skip(1).ToArray()));
        }

        private Dictionary<string, string> GetRequestParameters(string row)
        {
            if (string.IsNullOrEmpty(row)) return null;
            var kvs = Regex.Split(row, "&");
            if (kvs == null || kvs.Count() <= 0) return null;

            return kvs.ToDictionary(e => Regex.Split(e, "=")[0], e => { var p = Regex.Split(e, "="); return p.Length > 1 ? p[1] : ""; });
        }
    }
}
