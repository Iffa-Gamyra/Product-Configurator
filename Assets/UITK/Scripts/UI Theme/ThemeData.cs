using System;

[Serializable]
public class ThemeData
{
    public FontData fonts = new();
    public TextData text = new();
    public ColorData colors = new();
    public ImageData images = new();
}

[Serializable]
public class FontData
{
    public string bodyFontKey;
    public string boldFontKey;
    public string lightFontKey;

    public int bodyFontSize = 18;
    public int boldFontSize = 30;
    public int tabFontSize = 15;
    public int titleFontSize = 18;
    public int buttonFontSize = 16;
    public int smallFontSize = 10;
}

[Serializable]
public class TextData
{
    public string brandLogoText;

    public string welcomeDescription;

    public string startButtonText;

    public string homeTabProduct;
    public string homeTabVideo;

    public string mobileTabProducts;
    public string mobileTabVideo;

    public string topTabProduct;
    public string topTabSpecs;
    public string topTabInspect;

    public string specsSectionTitle;
    public string inspectSectionTitle;

    public string viewSpecsButton;
    public string inspectButton;
    public string doneButton;
    public string backNavLabel;

    public string resetViewLabel;
    public string prevNavLabel;
    public string nextNavLabel;

    public string downloadBrochureLabel;

    public string infoOverlayTitle;
    public string infoOverlayBody;
}

[Serializable]
public class ColorData
{
    public string accentPrimary;
    public string brandLogoColor;
    public string accentSecondary;

    public string primaryText;
    public string secondaryText;
    public string actionButtonText;

    public string welcomeBg;
    public string topNavBg;
    public string panelCardBg;
    public string overlayBackdropBg;
    public string infoCardBg;

    public string actionButtonBg;
    public string navIconActiveBg;
    public string navIconInactiveBg;

    public string dividerColor;
    public string progressBarFill;
    public string progressBarBg;
}

[Serializable]
public class ImageData
{
    public string startButtonBackground;
    public string homeTabButtonBackground;
    public string topTabActiveBanner;

    public string iconHome;
    public string iconProduct;
    public string iconVideo;
    public string iconInfo;

    public string iconSpecsBackNavButton;
    public string iconInspectBackNavButton;

    public string iconHide;
    public string iconScreenshot;

    public string iconFocus;
    public string iconResetView;
    public string iconPrevNav;
    public string iconNextNav;

    public string iconPlay;
    public string iconPause;
    public string iconMute;
    public string iconUnmute;
    public string iconReplay;

    public string iconDownload;
    public string iconClose;
}