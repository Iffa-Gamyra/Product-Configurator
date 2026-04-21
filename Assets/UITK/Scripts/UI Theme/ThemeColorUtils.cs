using UnityEngine;

public static class ThemeColorUtils
{
    public static Color Parse(string htmlColor, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(htmlColor))
            return fallback;

        return ColorUtility.TryParseHtmlString(htmlColor, out var color)
            ? color
            : fallback;
    }
}