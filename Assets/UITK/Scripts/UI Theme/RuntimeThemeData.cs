using UnityEngine;

[System.Serializable]
public class RuntimeThemeData
{
    public RuntimeFontGroup fonts = new();
    public TextData text = new();
    public RuntimeColorGroup colors = new();
    public RuntimeImageGroup images = new();

    public static RuntimeThemeData CreateDefault(ThemeFontLibrary fontLibrary = null)
    {
        return CreateFromJson(new ThemeData(), fontLibrary);
    }

    public static RuntimeThemeData CreateFromJson(ThemeData src, ThemeFontLibrary fontLibrary)
    {
        src ??= new ThemeData();

        return new RuntimeThemeData
        {
            text = src.text ?? new TextData(),
            fonts = new RuntimeFontGroup
            {
                bodyFont = fontLibrary != null ? fontLibrary.GetFont(src.fonts?.bodyFontKey) : null,
                boldFont = fontLibrary != null ? fontLibrary.GetFont(src.fonts?.boldFontKey) : null,
                lightFont = fontLibrary != null ? fontLibrary.GetFont(src.fonts?.lightFontKey) : null,
                bodyFontSize = src.fonts?.bodyFontSize ?? 18,
                boldFontSize = src.fonts?.boldFontSize ?? 30,
                tabFontSize = src.fonts?.tabFontSize ?? 15,
                titleFontSize = src.fonts?.titleFontSize ?? 18,
                buttonFontSize = src.fonts?.buttonFontSize ?? 16,
                smallFontSize = src.fonts?.smallFontSize ?? 10
            },
            colors = new RuntimeColorGroup
            {
                accentPrimary = ThemeColorUtils.Parse(src.colors?.accentPrimary, new Color(0.992f, 0.749f, 0.282f, 1f)),
                brandLogoColor = ThemeColorUtils.Parse(src.colors?.brandLogoColor, new Color(0.859f, 0.859f, 0.859f, 1f)),
                accentSecondary = ThemeColorUtils.Parse(src.colors?.accentSecondary, new Color(0.459f, 0.663f, 0.929f, 1f)),
                primaryText = ThemeColorUtils.Parse(src.colors?.primaryText, Color.white),
                secondaryText = ThemeColorUtils.Parse(src.colors?.secondaryText, new Color(0.471f, 0.471f, 0.471f, 1f)),
                actionButtonText = ThemeColorUtils.Parse(src.colors?.actionButtonText, new Color(0.012f, 0.137f, 0.114f, 1f)),
                welcomeBg = ThemeColorUtils.Parse(src.colors?.welcomeBg, new Color(0f, 0f, 0f, 0.99f)),
                topNavBg = ThemeColorUtils.Parse(src.colors?.topNavBg, new Color(0.114f, 0.114f, 0.114f, 1f)),
                panelCardBg = ThemeColorUtils.Parse(src.colors?.panelCardBg, new Color(0.102f, 0.102f, 0.102f, 0.9f)),
                overlayBackdropBg = ThemeColorUtils.Parse(src.colors?.overlayBackdropBg, new Color(0f, 0f, 0f, 0.47f)),
                infoCardBg = ThemeColorUtils.Parse(src.colors?.infoCardBg, new Color(0.118f, 0.118f, 0.118f, 0.53f)),
                actionButtonBg = ThemeColorUtils.Parse(src.colors?.actionButtonBg, new Color(0.906f, 0.906f, 0.906f, 1f)),
                navIconActiveBg = ThemeColorUtils.Parse(src.colors?.navIconActiveBg, new Color(0.922f, 0.498f, 0f, 0.63f)),
                navIconInactiveBg = ThemeColorUtils.Parse(src.colors?.navIconInactiveBg, new Color(0.153f, 0.149f, 0.149f, 0.51f)),
                dividerColor = ThemeColorUtils.Parse(src.colors?.dividerColor, new Color(0.820f, 0.820f, 0.820f, 1f)),
                progressBarFill = ThemeColorUtils.Parse(src.colors?.progressBarFill, new Color(0.384f, 0.690f, 0.973f, 1f)),
                progressBarBg = ThemeColorUtils.Parse(src.colors?.progressBarBg, new Color(0.392f, 0.392f, 0.392f, 0.78f))
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

    public int bodyFontSize = 18;
    public int boldFontSize = 30;
    public int tabFontSize = 15;
    public int titleFontSize = 18;
    public int buttonFontSize = 16;
    public int smallFontSize = 10;
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