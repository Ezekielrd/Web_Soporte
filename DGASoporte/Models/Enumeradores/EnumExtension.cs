using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DGASoporte.Models.Enumeradores
{
    public static class EnumExtension
    {
        public static string GetDisplayName(this Enum value)
        {
            if (value == null) return string.Empty;

            var type = value.GetType();
            var member = type.GetMember(value.ToString()).FirstOrDefault();
            if (member == null) return value.ToString();

            var attr = member.GetCustomAttribute<DisplayAttribute>();
            return attr?.Name ?? value.ToString();
        }
    }
}
