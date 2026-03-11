using UnityEngine;

public class DeviceDetection : MonoBehaviour
{
    public static DeviceDetection Instance { get; private set; }
    public static bool IsMobileActive { get; private set; }

    public enum UIMode
    {
        Auto,
        Desktop,
        Mobile
    }

    [Header("UI Roots")]
    [SerializeField] private GameObject desktopUI;
    [SerializeField] private GameObject mobileUI;

    [Header("Test Mode")]
    [SerializeField] private UIMode mode = UIMode.Auto;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        ApplyMode();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        ApplyMode();
    }
#endif

    private void ApplyMode()
    {
        bool useMobile = mode switch
        {
            UIMode.Desktop => false,
            UIMode.Mobile => true,
            _ => DetectMobile()
        };

        IsMobileActive = useMobile;

        if (mobileUI != null)
            mobileUI.SetActive(useMobile);

        if (desktopUI != null)
            desktopUI.SetActive(!useMobile);
    }

    private bool DetectMobile()
    {
        if (Application.isMobilePlatform)
            return true;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
            return Screen.width <= 900;

        return false;
    }
}