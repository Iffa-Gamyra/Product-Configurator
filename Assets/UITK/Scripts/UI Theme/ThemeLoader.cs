using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ThemeLoader : MonoBehaviour
{
    [SerializeField] private string cmsBaseUrl = "http://localhost:3000";

    public IEnumerator LoadTheme(
        string themeJsonPath,
        ThemeFontLibrary fontLibrary,
        Action<RuntimeThemeData> onSuccess,
        Action<string> onError = null)
    {
        string jsonUrl = BuildUrl(themeJsonPath);

        using var request = UnityWebRequest.Get(jsonUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"Failed to load theme JSON: {jsonUrl}\n{request.error}");
            yield break;
        }

        ThemeData themeData;
        try
        {
            themeData = JsonUtility.FromJson<ThemeData>(request.downloadHandler.text);
        }
        catch (Exception ex)
        {
            onError?.Invoke($"Invalid theme JSON: {jsonUrl}\n{ex.Message}");
            yield break;
        }

        if (themeData == null)
        {
            onError?.Invoke($"Theme JSON parsed to null: {jsonUrl}");
            yield break;
        }

        var runtime = RuntimeThemeData.CreateFromJson(themeData, fontLibrary);

        yield return LoadImages(themeData.images, runtime.images);

        onSuccess?.Invoke(runtime);
    }

    private IEnumerator LoadImages(ImageData src, RuntimeImageGroup dst)
    {
        if (src == null || dst == null)
            yield break;

        yield return LoadTexture(src.startButtonBackground, tex => dst.startButtonBackground = tex);
        yield return LoadTexture(src.homeTabButtonBackground, tex => dst.homeTabButtonBackground = tex);
        yield return LoadTexture(src.topTabActiveBanner, tex => dst.topTabActiveBanner = tex);

        yield return LoadTexture(src.iconHome, tex => dst.iconHome = tex);
        yield return LoadTexture(src.iconProduct, tex => dst.iconProduct = tex);
        yield return LoadTexture(src.iconVideo, tex => dst.iconVideo = tex);
        yield return LoadTexture(src.iconInfo, tex => dst.iconInfo = tex);

        yield return LoadTexture(src.iconSpecsBackNavButton, tex => dst.iconSpecsBackNavButton = tex);
        yield return LoadTexture(src.iconInspectBackNavButton, tex => dst.iconInspectBackNavButton = tex);

        yield return LoadTexture(src.iconHide, tex => dst.iconHide = tex);
        yield return LoadTexture(src.iconScreenshot, tex => dst.iconScreenshot = tex);

        yield return LoadTexture(src.iconFocus, tex => dst.iconFocus = tex);
        yield return LoadTexture(src.iconResetView, tex => dst.iconResetView = tex);
        yield return LoadTexture(src.iconPrevNav, tex => dst.iconPrevNav = tex);
        yield return LoadTexture(src.iconNextNav, tex => dst.iconNextNav = tex);

        yield return LoadTexture(src.iconPlay, tex => dst.iconPlay = tex);
        yield return LoadTexture(src.iconPause, tex => dst.iconPause = tex);
        yield return LoadTexture(src.iconMute, tex => dst.iconMute = tex);
        yield return LoadTexture(src.iconUnmute, tex => dst.iconUnmute = tex);
        yield return LoadTexture(src.iconReplay, tex => dst.iconReplay = tex);

        yield return LoadTexture(src.iconDownload, tex => dst.iconDownload = tex);
        yield return LoadTexture(src.iconClose, tex => dst.iconClose = tex);
    }

    private IEnumerator LoadTexture(string relativeOrAbsolutePath, Action<Texture2D> assign)
    {
        if (string.IsNullOrWhiteSpace(relativeOrAbsolutePath))
        {
            assign?.Invoke(null);
            yield break;
        }

        string url = BuildUrl(relativeOrAbsolutePath);

        using var request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Failed to load texture: {url}\n{request.error}");
            assign?.Invoke(null);
            yield break;
        }

        var texture = DownloadHandlerTexture.GetContent(request);
        assign?.Invoke(texture);
    }

    private string BuildUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return path;

        return $"{cmsBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}