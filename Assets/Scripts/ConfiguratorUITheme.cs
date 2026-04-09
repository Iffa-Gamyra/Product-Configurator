using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ConfiguratorUITheme", menuName = "Product Configurator/UI Theme")]
public class ConfiguratorUITheme : ScriptableObject
{
    public FontGroup fonts = new();
    public TextGroup text = new();
    public ColorGroup colors = new();
    public ImageGroup images = new();
}

[Serializable]
public class FontGroup
{
    [Header("Font Assets")]
    public Font bodyFont;
    public Font boldFont;
    public Font lightFont;

    [Header("Font Sizes")]
    public int bodyFontSize = 18;
    public int boldFontSize = 30;
    public int tabFontSize = 15;
    public int titleFontSize = 18;
    public int buttonFontSize = 16;
    public int smallFontSize = 10;
}

[Serializable]
public class TextGroup
{
    [Header("Brand")]
    public string brandLogoText = "GAMYRA";

    [Header("Welcome")]
    [TextArea]
    public string welcomeDescription =
        "This is an early version of our virtual showroom, where you can " +
        "explore and interact with our products.\n" +
        "For the full experience, please use a desktop computer.";
    public string startButtonText = "CLICK HERE TO START";

    [Header("Desktop Home")]
    public string homeTabProduct = "CHOOSE PRODUCT";
    public string homeTabVideo = "VIDEOS";

    [Header("Mobile Top Nav")]
    public string mobileTabProducts = "PRODUCTS";
    public string mobileTabVideo = "VIDEO";

    [Header("Desktop Top Tab Bar")]
    public string topTabProduct = "CHOOSE PRODUCT";
    public string topTabSpecs = "SPECS";
    public string topTabInspect = "INSPECT";

    [Header("Panel Titles")]
    public string specsSectionTitle = "SPECS";
    public string inspectSectionTitle = "INSPECT";

    [Header("Panel Buttons")]
    public string viewSpecsButton = "NEXT";
    public string inspectButton = "NEXT";
    public string doneButton = "DONE";
    public string backNavLabel = "BACK";

    [Header("Inspect Navigation")]
    public string resetViewLabel = "RESET VIEW";
    public string prevNavLabel = "VIEW PREV";
    public string nextNavLabel = "VIEW NEXT";

    [Header("Brochure")]
    public string downloadBrochureLabel = "DOWNLOAD BROCHURE";

    [Header("Info Overlay")]
    public string infoOverlayTitle = "INFO";

    [TextArea]
    public string infoOverlayBodyDesktop =
        "One-finger drag: Rotate model\n\n" +
        "Scroll: Zoom in or out\n\n" +
        "Inspect: Zoom into parts\n\n" +
        "Specs: View details\n\n" +
        "Replay: Restart video";

    [TextArea]
    public string infoOverlayBodyMobile =
        "Finger drag: Rotate model\n\n" +
        "Pinch: Zoom in/out\n\n" +
        "Inspect: Zoom into parts\n\n" +
        "Specs: View details\n\n" +
        "Replay: Restart video";
}

[Serializable]
public class ColorGroup
{
    [Header("Brand / Accent")]
    public Color accentPrimary = new Color(0.992f, 0.749f, 0.282f, 1f);
    public Color brandLogoColor = new Color(0.859f, 0.859f, 0.859f, 1f);

    [Tooltip("Used in USS hover/active pseudo-class states only. No runtime effect.")]
    public Color accentSecondary = new Color(0.459f, 0.663f, 0.929f, 1f);

    [Header("Text")]
    public Color primaryText = Color.white;
    public Color secondaryText = new Color(0.471f, 0.471f, 0.471f, 1f);
    public Color actionButtonText = new Color(0.012f, 0.137f, 0.114f, 1f);

    [Header("Backgrounds")]
    public Color welcomeBg = new Color(0f, 0f, 0f, 0.99f);
    public Color topNavBg = new Color(0.114f, 0.114f, 0.114f, 1f);
    public Color mobileNavBg = new Color(0f, 0f, 0f, 0.92f);
    public Color panelCardBg = new Color(0.102f, 0.102f, 0.102f, 0.9f);
    public Color overlayBackdropBg = new Color(0f, 0f, 0f, 0.47f);
    public Color infoCardBg = new Color(0.118f, 0.118f, 0.118f, 0.53f);

    [Header("Buttons")]
    public Color actionButtonBg = new Color(0.906f, 0.906f, 0.906f, 1f);
    public Color navIconActiveBg = new Color(0.922f, 0.498f, 0f, 0.63f);
    public Color navIconInactiveBg = new Color(0.153f, 0.149f, 0.149f, 0.51f);

    [Header("Misc")]
    public Color dividerColor = new Color(0.820f, 0.820f, 0.820f, 1f);
    public Color progressBarFill = new Color(0.384f, 0.690f, 0.973f, 1f);
    public Color progressBarBg = new Color(0.392f, 0.392f, 0.392f, 0.78f);
}

[Serializable]
public class ImageGroup
{
    [Header("Welcome")]
    public Texture2D startButtonBackground;

    [Header("Home")]
    public Texture2D homeTabButtonBackground;

    [Header("Top Tab Bar")]
    public Texture2D topTabActiveBanner;

    [Header("Side Nav Icons")]
    public Texture2D iconHome;
    public Texture2D iconProduct;
    public Texture2D iconVideo;
    public Texture2D iconInfo;

    [Header("Back Nav Buttons")]
    public Texture2D iconViewSpecsButton;
    public Texture2D iconInspectButton;

    [Header("Utility Buttons")]
    public Texture2D iconHide;
    public Texture2D iconScreenshot;

    [Header("Inspect")]
    public Texture2D iconFocus;
    public Texture2D iconResetView;
    public Texture2D iconPrevNav;
    public Texture2D iconNextNav;

    [Header("Video Controls")]
    public Texture2D iconPlay;
    public Texture2D iconPause;
    public Texture2D iconMute;
    public Texture2D iconUnmute;
    public Texture2D iconReplay;

    [Header("Download")]
    public Texture2D iconDownload;

    [Header("Info Overlay")]
    public Texture2D iconClose;
}
