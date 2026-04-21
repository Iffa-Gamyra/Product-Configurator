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

        // Coroutine yield must stay outside try/catch
        if (themeLoader != null)
        {
            yield return StartCoroutine(themeLoader.LoadTheme(
                themeFile,
                themeFontLibrary,
                theme => loadedTheme = theme,
                error => loadError = error));
        }

        RuntimeThemeData finalTheme = null;

        try
        {
            if (loadedTheme != null)
            {
                finalTheme = loadedTheme;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(loadError))
                {
                    Debug.LogWarning(loadError);
                    onError?.Invoke(loadError);
                }

                finalTheme = RuntimeThemeData.CreateDefault(themeFontLibrary);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Theme fallback failed. Using emergency defaults.\n{ex}");
            onError?.Invoke(ex.Message);

            finalTheme = new RuntimeThemeData();
        }

        onSuccess?.Invoke(finalTheme);
    }
}