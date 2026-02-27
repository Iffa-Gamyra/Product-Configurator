using System.Runtime.InteropServices;
using UnityEngine;

public class WebGLDownloadManager : MonoBehaviour
{
    public static WebGLDownloadManager Instance { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void VShowroom_DownloadPDFUrl(string url, string filename);
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DownloadPdfFromStreamingAssets(string pdfFileName, string downloadAsFileName = null)
    {
        if (string.IsNullOrWhiteSpace(pdfFileName))
        {
            Debug.LogWarning("DownloadPdfFromStreamingAssets: pdfFileName is empty.");
            return;
        }

        string url = $"{Application.streamingAssetsPath}/{pdfFileName}";
        string filename = string.IsNullOrWhiteSpace(downloadAsFileName) ? pdfFileName : downloadAsFileName;

#if UNITY_WEBGL && !UNITY_EDITOR
        VShowroom_DownloadPDFUrl(url, filename);
#else
        Application.OpenURL(url);
#endif
    }
}