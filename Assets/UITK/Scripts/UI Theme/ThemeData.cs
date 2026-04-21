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
    public string brandLogoText = "GAMYRA";

    public string welcomeDescription =
        "This is an early version of our virtual showroom, where you can " +
        "explore and interact with our products.\n" +
        "For the full experience, please use a desktop computer.";

    public string startButtonText = "CLICK HERE TO START";

    public string homeTabProduct = "CHOOSE PRODUCT";
    public string homeTabVideo = "VIDEOS";

    public string mobileTabProducts = "PRODUCTS";
    public string mobileTabVideo = "VIDEO";

    public string topTabProduct = "CHOOSE PRODUCT";
    public string topTabSpecs = "SPECS";
    public string topTabInspect = "INSPECT";

    public string specsSectionTitle = "SPECS";
    public string inspectSectionTitle = "INSPECT";

    public string viewSpecsButton = "NEXT";
    public string inspectButton = "NEXT";
    public string doneButton = "DONE";
    public string backNavLabel = "BACK";

    public string resetViewLabel = "RESET VIEW";
    public string prevNavLabel = "VIEW PREV";
    public string nextNavLabel = "VIEW NEXT";

    public string downloadBrochureLabel = "DOWNLOAD BROCHURE";

    public string infoOverlayTitle = "INFO";
    public string infoOverlayBody =
        "Mouse drag: Rotate model\n\n" +
        "Scroll: Zoom in or out\n\n" +
        "Inspect: Zoom into parts\n\n" +
        "Specs: View details\n\n" +
        "Replay: Restart video";
}

[Serializable]
public class ColorData
{
    public string accentPrimary = "#FDBF48";
    public string brandLogoColor = "#DBDBDB";
    public string accentSecondary = "#75A9ED";

    public string primaryText = "#FFFFFF";
    public string secondaryText = "#787878";
    public string actionButtonText = "#03231D";

    public string welcomeBg = "#000000FC";
    public string topNavBg = "#1D1D1D";
    public string panelCardBg = "#1A1A1AE6";
    public string overlayBackdropBg = "#00000078";
    public string infoCardBg = "#1E1E1E87";

    public string actionButtonBg = "#E7E7E7";
    public string navIconActiveBg = "#EB7F00A1";
    public string navIconInactiveBg = "#27262682";

    public string dividerColor = "#D1D1D1";
    public string progressBarFill = "#62B0F8";
    public string progressBarBg = "#646464C7";
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