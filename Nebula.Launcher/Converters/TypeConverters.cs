using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Nebula.Launcher.Converters;

public class TypeConverters
{
    public static FuncValueConverter<string, string?> IconConverter { get; } =
        new(iconKey =>
        {
            if (iconKey == null) return null;
            return $"/Assets/svg/{iconKey}.svg";
        });
}