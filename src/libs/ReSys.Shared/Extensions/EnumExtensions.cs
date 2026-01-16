using System.ComponentModel;
using System.Reflection;

namespace ReSys.Shared.Extensions;

public static class EnumExtensions
{
    public static string GetDescription<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        var field = typeof(TEnum).GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        
        return attribute?.Description ?? value.ToString();
    }

    public static List<string> GetDescriptions<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>()
            .Select(v => v.GetDescription())
            .ToList();
    }
}
