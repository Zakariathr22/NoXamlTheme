using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;

namespace NoXamlTheme;
public static class ThemeDebugHelper
{
    public static void DebugBrush(ResourceDictionary resources, string resourceKey)
    {
        LogBrushForDictionary(resources, resourceKey, "Default");
        LogBrushForDictionary(resources, resourceKey, "Light");
        LogBrushForDictionary(resources, resourceKey, "HighContrast");
        Debug.WriteLine(null);
    }

    private static void LogBrushForDictionary(ResourceDictionary resources, string key, string themeKey)
    {
        resources = new ColorPaletteResources();
        if (resources.ThemeDictionaries.TryGetValue(themeKey, out var dictObj) &&
            dictObj is ResourceDictionary dict &&
            dict.TryGetValue(key, out var value) &&
            value is SolidColorBrush brush)
        {
            var c = brush.Color;
            Debug.WriteLine($"{themeKey} → {key} = RGBA({c.R}, {c.G}, {c.B}, {c.A})");
        }
        else
        {
            Debug.WriteLine($"{themeKey} → {key} not found or not a SolidColorBrush");
        }
    }
}
