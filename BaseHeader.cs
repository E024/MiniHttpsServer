using System;
using System.Collections.Generic;
using System.Text;

namespace MiniHttpsServer
{
    internal class BaseHeader
    {
        internal string Body { get; set; }

        internal Encoding Encoding { get; set; }

        internal string Content_Type { get; set; }

        internal string Content_Length { get; set; }

        internal string Content_Encoding { get; set; }

        internal string ContentLanguage { get; set; }

        internal Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 不支持枚举类型约束，所以采取下列方案:)
        /// </summary>
        protected string GetHeaderByKey(Enum header)
        {
            if (Headers == null)
            {
                return null;
            }
            var fieldName = HeadersHelper.GetDescription(header);
            if (fieldName == null) return null;
            var hasKey = Headers.ContainsKey(fieldName);
            if (!hasKey) return null;
            return Headers[fieldName].Trim();
        }

        protected string GetHeaderByKey(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)) return null;
            var hasKey = Headers.ContainsKey(fieldName);
            if (!hasKey) return null;
            return Headers[fieldName];
        }

        /// <summary>
        /// 不支持枚举类型约束，所以采取下列方案:)
        /// </summary>
        protected void SetHeaderByKey(Enum header, string value)
        {
            var fieldName = HeadersHelper.GetDescription(header);
            if (fieldName == null) return;
            var hasKey = Headers.ContainsKey(fieldName);
            if (!hasKey) Headers.Add(fieldName, value);
            Headers[fieldName] = value;
        }

        protected void SetHeaderByKey(string fieldName, string value)
        {
            if (string.IsNullOrEmpty(fieldName)) return;
            var hasKey = Headers.ContainsKey(fieldName);
            if (!hasKey) Headers.Add(fieldName, value);
            Headers[fieldName] = value;
        }
    }
}
