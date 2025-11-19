using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DGASoporte.Models.Enumeradores
{
    public static class EnumExtension
    {
       public static string GetDisplayName(Enum enumValue)
        {
        
            if (enumValue == null) return "No definido";

            var displayAttribute = enumValue.GetType()
                                .GetMember(enumValue.ToString())[0]
                                .GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.Name ?? enumValue.ToString();
       }
    }
}
