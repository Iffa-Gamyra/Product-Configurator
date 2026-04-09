using UnityEngine;
using UnityEngine.UIElements;

public class UIThemeApplicator
{
    private readonly HomeScreenUI ui;
    private readonly bool isMobileLayout;

    public UIThemeApplicator(HomeScreenUI ui, bool isMobileLayout)
    {
        this.ui = ui;
        this.isMobileLayout = isMobileLayout;
    }

    public void Apply(ConfiguratorUITheme theme)
    {
        if (theme == null || ui == null) return;
        ApplyText(theme.text);
        ApplyFonts(theme.fonts);
        ApplyColors(theme.colors);
        ApplyImages(theme.images);
        ApplyLayoutVariants();
    }

    private void ApplyText(TextGroup t)
    {
        SetText(ui.WelcomeTitleLabel, t.brandLogoText);
        SetText(ui.WelcomeDescLabel, t.welcomeDescription);
        SetText(ui.WelcomeStartBtn, t.startButtonText);

        SetText(ui.HomeProductTabBtn, t.homeTabProduct);
        SetText(ui.HomeVideoTabBtn, t.homeTabVideo);
        SetText(ui.HomeLogoLabel, t.brandLogoText);
        SetText(ui.UtilsLogoLabel, t.brandLogoText);

        foreach (var b in ui.SpecsTabButtons) b.text = t.topTabSpecs;
        foreach (var b in ui.InspectTabButtons) b.text = t.topTabInspect;

        foreach (var l in ui.BannerLabels)
        {
            if (l.ClassListContains("tab-active-banner-label"))
            {
                if (l.name == UINames.TopNav_Inspect || l.parent?.name?.Contains("Inspect") == true)
                    l.text = t.topTabInspect;
                else if (l.name == UINames.TopNav_Specs || l.parent?.name?.Contains("Specs") == true)
                    l.text = t.topTabSpecs;
                else
                    l.text = t.topTabProduct;
            }
        }

        foreach (var b in ui.MobileHomeTabBtns) b.text = t.mobileTabProducts;
        foreach (var b in ui.MobileVideoTabBtns) b.text = t.mobileTabVideo;

        SetText(ui.SpecsTitleLabel, t.specsSectionTitle);
        SetText(ui.InspectTitleLabel, t.inspectSectionTitle);

        SetText(ui.SpecsBackNavLabel, t.backNavLabel);
        SetText(ui.InspectBackNavLabel, t.backNavLabel);

        SetText(ui.SpecsButton, t.viewSpecsButton);
        SetText(ui.InspectButton, t.inspectButton);
        SetText(ui.InspectDoneBtn, t.doneButton);

        if (ui.ResetViewButton != null) ui.ResetViewButton.text = "";
        SetText(ui.InspectResetLabel, t.resetViewLabel);
        SetText(ui.InspectPrevLabel, t.prevNavLabel);
        SetText(ui.InspectNextLabel, t.nextNavLabel);

        SetText(ui.SpecsDownloadLabel, t.downloadBrochureLabel);

        SetText(ui.InfoTitleLabel, t.infoOverlayTitle);
        SetText(ui.InfoBodyLabel,
            isMobileLayout ? t.infoOverlayBodyMobile : t.infoOverlayBodyDesktop);
    }

    private void ApplyFonts(FontGroup f)
    {
        if (f == null) return;
        var t = ui.Targets;

        if (f.bodyFont != null) foreach (var e in t.FontBody) e.style.unityFont = f.bodyFont;
        if (f.boldFont != null) foreach (var e in t.FontBold) e.style.unityFont = f.boldFont;
        if (f.lightFont != null) foreach (var e in t.FontLight) e.style.unityFont = f.lightFont;

        if (f.bodyFontSize != 18) foreach (var e in t.TextBase) e.style.fontSize = f.bodyFontSize;
        if (f.boldFontSize != 30) foreach (var e in t.Text2XL) e.style.fontSize = f.boldFontSize;
        if (f.tabFontSize != 15) foreach (var e in t.TextSM) e.style.fontSize = f.tabFontSize;
        if (f.titleFontSize != 18) foreach (var e in t.TextTitle) e.style.fontSize = f.titleFontSize;
        if (f.buttonFontSize != 16) foreach (var e in t.TextMD) e.style.fontSize = f.buttonFontSize;
        if (f.smallFontSize != 10) foreach (var e in t.TextXS) e.style.fontSize = f.smallFontSize;
    }

    private void ApplyColors(ColorGroup c)
    {
        if (c == null) return;
        var t = ui.Targets;

        foreach (var e in t.ColorPrimary) e.style.color = c.primaryText;
        foreach (var e in t.ColorBrand) e.style.color = c.brandLogoColor;
        foreach (var e in t.ColorAccent) e.style.color = c.accentPrimary;
        foreach (var e in t.ColorMuted) e.style.color = c.secondaryText;
        foreach (var e in t.ColorDark) e.style.color = c.actionButtonText;

        foreach (var e in t.Dividers) e.style.backgroundColor = c.dividerColor;

        SetBg(ui.WelcomeScreen, c.welcomeBg);
        SetBg(ui.InfoCard, c.infoCardBg);
        SetBg(ui.InfoOverlay, c.overlayBackdropBg);

        foreach (var e in ui.TopNavContainers) e.style.backgroundColor = c.topNavBg;
        foreach (var e in ui.MobileNavContainers) e.style.backgroundColor = c.mobileNavBg;

        if (!isMobileLayout)
        {
            SetBg(ui.SpecsSectionRoot, c.panelCardBg);
            SetBg(ui.InspectSectionRoot, c.panelCardBg);
        }

        SetActionBg(ui.SpecsButton, c.actionButtonBg);
        SetActionBg(ui.InspectButton, c.actionButtonBg);
        SetActionBg(ui.InspectDoneBtn, c.actionButtonBg);

        foreach (var b in ui.SideNavHomeBtns) b.style.backgroundColor = c.navIconInactiveBg;
        foreach (var b in ui.SideNavProductBtns) b.style.backgroundColor = c.navIconInactiveBg;
        foreach (var b in ui.SideNavVideoBtns) b.style.backgroundColor = c.navIconInactiveBg;
        foreach (var b in ui.SideNavInfoBtns) b.style.backgroundColor = c.navIconInactiveBg;

        if (ui.ProgressBarFill != null) ui.ProgressBarFill.style.backgroundColor = c.progressBarFill;
        if (ui.ProgressBarTrack != null) ui.ProgressBarTrack.style.backgroundColor = c.progressBarBg;
    }

    private void ApplyImages(ImageGroup img)
    {
        if (img == null) return;

        SetImg(ui.WelcomeStartBtn, img.startButtonBackground);
        SetImg(ui.HomeProductTabBtn, img.homeTabButtonBackground);
        SetImg(ui.HomeVideoTabBtn, img.homeTabButtonBackground);

        foreach (var b in ui.SideNavHomeBtns) SetImg(b, img.iconHome);
        foreach (var b in ui.SideNavProductBtns) SetImg(b, img.iconProduct);
        foreach (var b in ui.SideNavVideoBtns) SetImg(b, img.iconVideo);
        foreach (var b in ui.SideNavInfoBtns) SetImg(b, img.iconInfo);

        if (img.topTabActiveBanner != null)
        {
            var bg = new StyleBackground(img.topTabActiveBanner);
            foreach (var l in ui.BannerLabels) l.style.backgroundImage = bg;
        }

        SetImg(ui.UtilsHideBtn, img.iconHide);
        SetImg(ui.UtilsScreenshotBtn, img.iconScreenshot);

        SetImg(ui.PlayButton, img.iconPlay);
        SetImg(ui.PauseButton, img.iconPause);
        SetImg(ui.MuteButton, img.iconMute);
        SetImg(ui.UnmuteButton, img.iconUnmute);
        SetImg(ui.ReplayButton, img.iconReplay);

        SetImg(ui.InspectResetIcon, img.iconResetView);
        SetImg(ui.InspectPrevButton, img.iconPrevNav);
        SetImg(ui.InspectNextButton, img.iconNextNav);

        SetImg(ui.DownloadPdfButton, img.iconDownload);
        SetImg(ui.InfoCloseBtn, img.iconClose);

        SetImg(ui.SpecsButton, img.iconViewSpecsButton);
        SetImg(ui.InspectDoneBtn, img.iconInspectButton);
    }

    private void ApplyLayoutVariants()
    {
        if (ui.InfoCard == null) return;
        ui.InfoCard.EnableInClassList(UINames.Class_InfoCardDesktop, !isMobileLayout);
        ui.InfoCard.EnableInClassList(UINames.Class_InfoCardMobile, isMobileLayout);
    }

    private static void SetText(Label l, string v) { if (l != null) l.text = v; }
    private static void SetText(Button b, string v) { if (b != null) b.text = v; }
    private static void SetBg(VisualElement e, Color c) { if (e != null) e.style.backgroundColor = c; }
    private static void SetActionBg(Button b, Color c) { if (b != null) b.style.backgroundColor = c; }
    private static void SetImg(VisualElement e, Texture2D tex)
    {
        if (e != null && tex != null)
            e.style.backgroundImage = new StyleBackground(tex);
    }
}
