using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class ScreenshotHandler : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void VShowroom_DownloadScreenshot(string str);

    public static ScreenshotHandler Instance { get; private set; }

    private void Awake()
    {
        // Ensure that there's only one instance of ScreenshotManager
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

    public void CaptureScreenshot()
    {
        StartCoroutine(TakeScreenshotAndDownload());
    }

    private IEnumerator TakeScreenshotAndDownload()
    {
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();
        string base64 = System.Convert.ToBase64String(bytes);

        VShowroom_DownloadScreenshot(base64);

        Destroy(texture);
    }
}
