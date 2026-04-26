using UnityEngine;

[System.Serializable]
public class RuntimeThemeData
{
    public RuntimeFontGroup fonts = new();
    public TextData text = new();
    public RuntimeColorGroup colors = new();
    public RuntimeImageGroup images = new();

    public static RuntimeThemeData CreateFromJson(ThemeData src, ThemeFontLibrary fontLibrary)
    {
        return new RuntimeThemeData
        {
            text = src.text,

            fonts = new RuntimeFontGroup
            {
                bodyFont = fontLibrary.GetFont(src.fonts.bodyFontKey),
                boldFont = fontLibrary.GetFont(src.fonts.boldFontKey),
                lightFont = fontLibrary.GetFont(src.fonts.lightFontKey),

                bodyFontSize = src.fonts.bodyFontSize,
                boldFontSize = src.fonts.boldFontSize,
                tabFontSize = src.fonts.tabFontSize,
                titleFontSize = src.fonts.titleFontSize,
                buttonFontSize = src.fonts.buttonFontSize,
                smallFontSize = src.fonts.smallFontSize
            },

            colors = new RuntimeColorGroup
            {
                accentPrimary = ThemeColorUtils.Parse(src.colors.accentPrimary, Color.white),
                brandLogoColor = ThemeColorUtils.Parse(src.colors.brandLogoColor, Color.white),
                accentSecondary = ThemeColorUtils.Parse(src.colors.accentSecondary, Color.white),

                primaryText = ThemeColorUtils.Parse(src.colors.primaryText, Color.white),
                secondaryText = ThemeColorUtils.Parse(src.colors.secondaryText, Color.gray),
                actionButtonText = ThemeColorUtils.Parse(src.colors.actionButtonText, Color.black),

                welcomeBg = ThemeColorUtils.Parse(src.colors.welcomeBg, Color.black),
                topNavBg = ThemeColorUtils.Parse(src.colors.topNavBg, Color.black),
                panelCardBg = ThemeColorUtils.Parse(src.colors.panelCardBg, Color.black),
                overlayBackdropBg = ThemeColorUtils.Parse(src.colors.overlayBackdropBg, Color.black),
                infoCardBg = ThemeColorUtils.Parse(src.colors.infoCardBg, Color.black),

                actionButtonBg = ThemeColorUtils.Parse(src.colors.actionButtonBg, Color.white),
                navIconActiveBg = ThemeColorUtils.Parse(src.colors.navIconActiveBg, Color.white),
                navIconInactiveBg = ThemeColorUtils.Parse(src.colors.navIconInactiveBg, Color.gray),

                dividerColor = ThemeColorUtils.Parse(src.colors.dividerColor, Color.gray),
                progressBarFill = ThemeColorUtils.Parse(src.colors.progressBarFill, Color.white),
                progressBarBg = ThemeColorUtils.Parse(src.colors.progressBarBg, Color.gray)
            },

            images = new RuntimeImageGroup()
        };
    }
}

[System.Serializable]
public class RuntimeFontGroup
{
    public Font bodyFont;
    public Font boldFont;
    public Font lightFont;

    public int bodyFontSize;
    public int boldFontSize;
    public int tabFontSize;
    public int titleFontSize;
    public int buttonFontSize;
    public int smallFontSize;
}

[System.Serializable]
public class RuntimeColorGroup
{
    public Color accentPrimary;
    public Color brandLogoColor;
    public Color accentSecondary;

    public Color primaryText;
    public Color secondaryText;
    public Color actionButtonText;

    public Color welcomeBg;
    public Color topNavBg;
    public Color panelCardBg;
    public Color overlayBackdropBg;
    public Color infoCardBg;

    public Color actionButtonBg;
    public Color navIconActiveBg;
    public Color navIconInactiveBg;

    public Color dividerColor;
    public Color progressBarFill;
    public Color progressBarBg;
}

[System.Serializable]
public class RuntimeImageGroup
{
    public Texture2D startButtonBackground;
    public Texture2D homeTabButtonBackground;
    public Texture2D topTabActiveBanner;

    public Texture2D iconHome;
    public Texture2D iconProduct;
    public Texture2D iconVideo;
    public Texture2D iconInfo;

    public Texture2D iconSpecsBackNavButton;
    public Texture2D iconInspectBackNavButton;

    public Texture2D iconHide;
    public Texture2D iconScreenshot;

    public Texture2D iconFocus;
    public Texture2D iconResetView;
    public Texture2D iconPrevNav;
    public Texture2D iconNextNav;

    public Texture2D iconPlay;
    public Texture2D iconPause;
    public Texture2D iconMute;
    public Texture2D iconUnmute;
    public Texture2D iconReplay;

    public Texture2D iconDownload;
    public Texture2D iconClose;
}