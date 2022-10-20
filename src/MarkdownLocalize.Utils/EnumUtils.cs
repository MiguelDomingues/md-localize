using System;
using System.Runtime.Serialization;
using System.Linq;
using System.Reflection;
using System.ComponentModel;

namespace MarkdownLocalize.Utils
{

    public static class EnumUtils
    {
        public static string GetEnumMemberAttrValue(object enumVal)
        {
            Type enumType = enumVal.GetType();
            var memInfo = enumType.GetMember(enumVal.ToString());
            var attr = memInfo[0].GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();
            if (attr != null)
            {
                return attr.Value;
            }

            return null;
        }

        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();

            string name = Enum.GetName(type, value);

            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }

            return name;
        }
    }
}