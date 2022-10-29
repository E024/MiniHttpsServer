using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MiniHttpsServer
{
    internal class HeadersHelper
    {
        internal static string GetDescription(Enum value)
        {
            var valueType = value.GetType();
            var memberName = Enum.GetName(valueType, value);
            if (memberName == null) return null;
            var fieldInfo = valueType.GetField(memberName);
            var attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
            if (attribute == null) return null;
            return (attribute as DescriptionAttribute).Description;
        }
    }
}
