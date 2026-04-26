using System;
using System.Collections;
using UnityEngine;

public class HomeScreenThemeBootstrap : MonoBehaviour
{
    [SerializeField] private ThemeLoader themeLoader;
    [SerializeField] private ThemeFontLibrary themeFontLibrary;
    [SerializeField] private string desktopThemeFile = "themes/desktop-theme.json";
    [SerializeField] private string mobileThemeFile = "themes/mobile-theme.json";

    public ThemeFontLibrary FontLibrary => themeFontLibrary;

    public IEnumerator LoadThemeForCurrentDevice(
        bool isMobileLayout,
        Action<RuntimeThemeData> onSuccess,
        Action<string> onError = null,
        Action<float> onProgress = null)
    {
        if (themeLoader == null)
        {
            onError?.Invoke("ThemeLoader is not assigned.");
            yield break;
        }

        if (themeFontLibrary == null)
        {
            onError?.Invoke("ThemeFontLibrary is not assigned.");
            yield break;
        }

        string themeFile = isMobileLayout ? mobileThemeFile : desktopThemeFile;

        RuntimeThemeData loadedTheme = null;
        string loadError = null;

        yield return StartCoroutine(themeLoader.LoadTheme(
            themeFile,
            themeFontLibrary,
            theme => loadedTheme = theme,
            error => loadError = error,
            onProgress));

        if (loadedTheme == null)
        {
            onError?.Invoke(string.IsNullOrWhiteSpace(loadError)
                ? $"Could not load theme: {themeFile}"
                : loadError);
            yield break;
        }

        onSuccess?.Invoke(loadedTheme);
    }
}
