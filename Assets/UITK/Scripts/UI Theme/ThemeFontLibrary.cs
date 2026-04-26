using System;
using System.Collections.Generic;
using UnityEngine;

public class ThemeFontLibrary : MonoBehaviour
{
    [Serializable]
    public class FontEntry
    {
        public string key;
        public Font font;
    }

    [SerializeField] private List<FontEntry> fonts = new();

    private Dictionary<string, Font> fontMap;

    private void Awake()
    {
        fontMap = new Dictionary<string, Font>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in fonts)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.key) || entry.font == null)
                continue;

            fontMap[entry.key] = entry.font;
        }
    }

    public Font GetFont(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || fontMap == null)
            return null;

        fontMap.TryGetValue(key, out var font);
        return font;
    }

    public bool HasFont(string key)
    {
        return !string.IsNullOrWhiteSpace(key)
               && fontMap != null
               && fontMap.ContainsKey(key);
    }
}