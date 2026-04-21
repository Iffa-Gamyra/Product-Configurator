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
        Action<string> onError = null)
    {
        string themeFile = isMobileLayout ? mobileThemeFile : desktopThemeFile;

        RuntimeThemeData loadedTheme = null;
        string loadError = null;

        if (themeLoader != null)
        {
            yield return StartCoroutine(themeLoader.LoadTheme(
                themeFile,
                themeFontLibrary,
                theme => loadedTheme = theme,
                error => loadError = error));
        }

        if (loadedTheme == null)
        {
            if (!string.IsNullOrWhiteSpace(loadError))
            {
                Debug.LogError(loadError);
                onError?.Invoke(loadError);
            }

            loadedTheme = RuntimeThemeData.CreateDefault(themeFontLibrary);
        }

        onSuccess?.Invoke(loadedTheme);
    }
}