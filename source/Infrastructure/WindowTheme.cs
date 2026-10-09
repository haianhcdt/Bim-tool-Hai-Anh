using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace DSCons.Revit.Starter.Infrastructure;

/// <summary>Attaches the shared branded WPF theme to a tool window.</summary>
public static class WindowTheme
{
    public static void Apply(Window window)
    {
        if (window is null) throw new ArgumentNullException(nameof(window));
        window.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/DSCons.Revit.Starter;component/Resources/Theme.xaml", UriKind.Relative)
        });

        var accent = ParseBrandColor(StudentBranding.PrimaryColor);
        window.Resources["AccentBrush"] = new SolidColorBrush(accent);
        window.Resources["AccentLightBrush"] = new SolidColorBrush(Blend(accent, Colors.White, 0.90));
        window.Resources["AccentDarkBrush"] = new SolidColorBrush(Blend(accent, Colors.Black, 0.22));
        window.Resources["AccentForegroundBrush"] = new SolidColorBrush(GetReadableForeground(accent));
    }

    private static Color ParseBrandColor(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length == 7 && value[0] == '#'
            && byte.TryParse(value.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var red)
            && byte.TryParse(value.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var green)
            && byte.TryParse(value.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var blue))
        {
            return Color.FromRgb(red, green, blue);
        }

        return Color.FromRgb(0, 123, 153);
    }

    private static Color Blend(Color source, Color target, double targetWeight)
    {
        var sourceWeight = 1d - targetWeight;
        return Color.FromRgb(
            (byte)Math.Round((source.R * sourceWeight) + (target.R * targetWeight)),
            (byte)Math.Round((source.G * sourceWeight) + (target.G * targetWeight)),
            (byte)Math.Round((source.B * sourceWeight) + (target.B * targetWeight)));
    }

    private static Color GetReadableForeground(Color background)
    {
        var luminance = ((0.299 * background.R) + (0.587 * background.G) + (0.114 * background.B)) / 255d;
        return luminance > 0.60 ? Color.FromRgb(15, 23, 42) : Colors.White;
    }
}