using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ThemeLoader : MonoBehaviour
{
    [SerializeField] private string cmsBaseUrl = "http://localhost:3000";

    private const int IMAGE_COUNT = 22;

    public IEnumerator LoadTheme(
        string themeJsonPath,
        ThemeFontLibrary fontLibrary,
        Action<RuntimeThemeData> onSuccess,
        Action<string> onError = null,
        Action<float> onProgress = null)
    {
        string jsonUrl = BuildUrl(themeJsonPath);

        onProgress?.Invoke(0.05f);

        using var request = UnityWebRequest.Get(jsonUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"Theme JSON could not be loaded:\n{jsonUrl}\n{request.error}");
            yield break;
        }

        onProgress?.Invoke(0.25f);

        ThemeData themeData;
        try
        {
            themeData = JsonUtility.FromJson<ThemeData>(request.downloadHandler.text);
        }
        catch (Exception ex)
        {
            onError?.Invoke($"Theme JSON is invalid:\n{jsonUrl}\n{ex.Message}");
            yield break;
        }

        if (!ThemeValidator.IsValid(themeData, fontLibrary, out var validationError))
        {
            onError?.Invoke(validationError);
            yield break;
        }

        onProgress?.Invoke(0.35f);

        var runtime = RuntimeThemeData.CreateFromJson(themeData, fontLibrary);

        onProgress?.Invoke(0.40f);

        string imageError = null;
        int imagesLoaded = 0;

        yield return LoadImages(themeData.images, runtime.images,
            err => imageError = err,
            () =>
            {
                imagesLoaded++;
                float imageProgress = 0.40f + (imagesLoaded / (float)IMAGE_COUNT) * 0.55f;
                onProgress?.Invoke(imageProgress);
            });

        if (!string.IsNullOrWhiteSpace(imageError))
        {
            onError?.Invoke(imageError);
            yield break;
        }

        onProgress?.Invoke(1f);
        onSuccess?.Invoke(runtime);
    }

    private IEnumerator LoadImages(
        ImageData src,
        RuntimeImageGroup dst,
        Action<string> onError,
        Action onImageLoaded)
    {
        string err = null;

        yield return LoadTexture(src.startButtonBackground, tex => dst.startButtonBackground = tex, e => err = e, "startButtonBackground", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.homeTabButtonBackground, tex => dst.homeTabButtonBackground = tex, e => err = e, "homeTabButtonBackground", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.topTabActiveBanner, tex => dst.topTabActiveBanner = tex, e => err = e, "topTabActiveBanner", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconHome, tex => dst.iconHome = tex, e => err = e, "iconHome", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconProduct, tex => dst.iconProduct = tex, e => err = e, "iconProduct", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconVideo, tex => dst.iconVideo = tex, e => err = e, "iconVideo", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconInfo, tex => dst.iconInfo = tex, e => err = e, "iconInfo", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconSpecsBackNavButton, tex => dst.iconSpecsBackNavButton = tex, e => err = e, "iconSpecsBackNavButton", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconInspectBackNavButton, tex => dst.iconInspectBackNavButton = tex, e => err = e, "iconInspectBackNavButton", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconHide, tex => dst.iconHide = tex, e => err = e, "iconHide", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconScreenshot, tex => dst.iconScreenshot = tex, e => err = e, "iconScreenshot", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconFocus, tex => dst.iconFocus = tex, e => err = e, "iconFocus", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconResetView, tex => dst.iconResetView = tex, e => err = e, "iconResetView", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconPrevNav, tex => dst.iconPrevNav = tex, e => err = e, "iconPrevNav", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconNextNav, tex => dst.iconNextNav = tex, e => err = e, "iconNextNav", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconPlay, tex => dst.iconPlay = tex, e => err = e, "iconPlay", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconPause, tex => dst.iconPause = tex, e => err = e, "iconPause", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconMute, tex => dst.iconMute = tex, e => err = e, "iconMute", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconUnmute, tex => dst.iconUnmute = tex, e => err = e, "iconUnmute", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconReplay, tex => dst.iconReplay = tex, e => err = e, "iconReplay", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconDownload, tex => dst.iconDownload = tex, e => err = e, "iconDownload", onImageLoaded);
        if (err != null) { onError(err); yield break; }

        yield return LoadTexture(src.iconClose, tex => dst.iconClose = tex, e => err = e, "iconClose", onImageLoaded);
        if (err != null) { onError(err); yield break; }
    }

    private IEnumerator LoadTexture(
        string path,
        Action<Texture2D> assign,
        Action<string> onError,
        string fieldName,
        Action onLoaded = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            onError?.Invoke($"Required image path is missing: {fieldName}");
            yield break;
        }

        string url = BuildUrl(path);
        using var request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"Required image failed to load: {fieldName}\n{url}\n{request.error}");
            yield break;
        }

        assign?.Invoke(DownloadHandlerTexture.GetContent(request));
        onLoaded?.Invoke();
    }

    private string BuildUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return path;

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return path;

        return $"{cmsBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}
