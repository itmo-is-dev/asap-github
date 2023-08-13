using System.Globalization;

namespace Itmo.Dev.Asap.Github.Common.Tools;

public class RuCultureDate
{
    public static DateOnly Parse(string? value)
    {
        return !DateOnly.TryParse(value, CultureInfo.GetCultureInfo("ru-Ru"), DateTimeStyles.None, out DateOnly date)
            ? throw new InvalidOperationException(
                $"Cannot parse input date ({value} as date. Ensure that you use correct format.")
            : date;
    }
}