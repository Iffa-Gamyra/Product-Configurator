using System.Collections.Generic;
using UnityEngine;

public static class ThemeValidator
{
    public static bool IsValid(ThemeData theme, ThemeFontLibrary fontLibrary, out string error)
    {
        var missing = new List<string>();

        if (theme == null)
        {
            error = "Theme JSON could not be parsed.";
            return false;
        }

        if (theme.fonts == null) missing.Add("fonts");
        if (theme.text == null) missing.Add("text");
        if (theme.colors == null) missing.Add("colors");
        if (theme.images == null) missing.Add("images");

        if (theme.fonts != null)
        {
            Require(theme.fonts.bodyFontKey, "fonts.bodyFontKey", missing);
            Require(theme.fonts.boldFontKey, "fonts.boldFontKey", missing);
            Require(theme.fonts.lightFontKey, "fonts.lightFontKey", missing);

            if (fontLibrary == null)
            {
                missing.Add("ThemeFontLibrary is not assigned.");
            }
            else
            {
                RequireFont(fontLibrary, theme.fonts.bodyFontKey, "fonts.bodyFontKey", missing);
                RequireFont(fontLibrary, theme.fonts.boldFontKey, "fonts.boldFontKey", missing);
                RequireFont(fontLibrary, theme.fonts.lightFontKey, "fonts.lightFontKey", missing);
            }
        }

        if (theme.text != null)
        {
            Require(theme.text.brandLogoText, "text.brandLogoText", missing);
            Require(theme.text.welcomeDescription, "text.welcomeDescription", missing);
            Require(theme.text.startButtonText, "text.startButtonText", missing);

            Require(theme.text.homeTabProduct, "text.homeTabProduct", missing);
            Require(theme.text.homeTabVideo, "text.homeTabVideo", missing);

            Require(theme.text.mobileTabProducts, "text.mobileTabProducts", missing);
            Require(theme.text.mobileTabVideo, "text.mobileTabVideo", missing);

            Require(theme.text.topTabProduct, "text.topTabProduct", missing);
            Require(theme.text.topTabSpecs, "text.topTabSpecs", missing);
            Require(theme.text.topTabInspect, "text.topTabInspect", missing);

            Require(theme.text.specsSectionTitle, "text.specsSectionTitle", missing);
            Require(theme.text.inspectSectionTitle, "text.inspectSectionTitle", missing);

            Require(theme.text.viewSpecsButton, "text.viewSpecsButton", missing);
            Require(theme.text.inspectButton, "text.inspectButton", missing);
            Require(theme.text.doneButton, "text.doneButton", missing);
            Require(theme.text.backNavLabel, "text.backNavLabel", missing);

            Require(theme.text.resetViewLabel, "text.resetViewLabel", missing);
            Require(theme.text.prevNavLabel, "text.prevNavLabel", missing);
            Require(theme.text.nextNavLabel, "text.nextNavLabel", missing);

            Require(theme.text.downloadBrochureLabel, "text.downloadBrochureLabel", missing);

            Require(theme.text.infoOverlayTitle, "text.infoOverlayTitle", missing);
            Require(theme.text.infoOverlayBody, "text.infoOverlayBody", missing);
        }

        if (theme.colors != null)
        {
            RequireColor(theme.colors.accentPrimary, "colors.accentPrimary", missing);
            RequireColor(theme.colors.brandLogoColor, "colors.brandLogoColor", missing);
            RequireColor(theme.colors.accentSecondary, "colors.accentSecondary", missing);

            RequireColor(theme.colors.primaryText, "colors.primaryText", missing);
            RequireColor(theme.colors.secondaryText, "colors.secondaryText", missing);
            RequireColor(theme.colors.actionButtonText, "colors.actionButtonText", missing);

            RequireColor(theme.colors.welcomeBg, "colors.welcomeBg", missing);
            RequireColor(theme.colors.topNavBg, "colors.topNavBg", missing);
            RequireColor(theme.colors.panelCardBg, "colors.panelCardBg", missing);
            RequireColor(theme.colors.overlayBackdropBg, "colors.overlayBackdropBg", missing);
            RequireColor(theme.colors.infoCardBg, "colors.infoCardBg", missing);

            RequireColor(theme.colors.actionButtonBg, "colors.actionButtonBg", missing);
            RequireColor(theme.colors.navIconActiveBg, "colors.navIconActiveBg", missing);
            RequireColor(theme.colors.navIconInactiveBg, "colors.navIconInactiveBg", missing);

            RequireColor(theme.colors.dividerColor, "colors.dividerColor", missing);
            RequireColor(theme.colors.progressBarFill, "colors.progressBarFill", missing);
            RequireColor(theme.colors.progressBarBg, "colors.progressBarBg", missing);
        }

        if (theme.images != null)
        {
            Require(theme.images.startButtonBackground, "images.startButtonBackground", missing);
            Require(theme.images.homeTabButtonBackground, "images.homeTabButtonBackground", missing);
            Require(theme.images.topTabActiveBanner, "images.topTabActiveBanner", missing);

            Require(theme.images.iconHome, "images.iconHome", missing);
            Require(theme.images.iconProduct, "images.iconProduct", missing);
            Require(theme.images.iconVideo, "images.iconVideo", missing);
            Require(theme.images.iconInfo, "images.iconInfo", missing);

            Require(theme.images.iconSpecsBackNavButton, "images.iconSpecsBackNavButton", missing);
            Require(theme.images.iconInspectBackNavButton, "images.iconInspectBackNavButton", missing);

            Require(theme.images.iconHide, "images.iconHide", missing);
            Require(theme.images.iconScreenshot, "images.iconScreenshot", missing);

            Require(theme.images.iconFocus, "images.iconFocus", missing);
            Require(theme.images.iconResetView, "images.iconResetView", missing);
            Require(theme.images.iconPrevNav, "images.iconPrevNav", missing);
            Require(theme.images.iconNextNav, "images.iconNextNav", missing);

            Require(theme.images.iconPlay, "images.iconPlay", missing);
            Require(theme.images.iconPause, "images.iconPause", missing);
            Require(theme.images.iconMute, "images.iconMute", missing);
            Require(theme.images.iconUnmute, "images.iconUnmute", missing);
            Require(theme.images.iconReplay, "images.iconReplay", missing);

            Require(theme.images.iconDownload, "images.iconDownload", missing);
            Require(theme.images.iconClose, "images.iconClose", missing);
        }

        if (missing.Count > 0)
        {
            error = "Theme JSON is invalid or incomplete:\n- " + string.Join("\n- ", missing);
            return false;
        }

        error = null;
        return true;
    }

    private static void Require(string value, string field, List<string> missing)
    {
        if (string.IsNullOrWhiteSpace(value))
            missing.Add(field);
    }

    private static void RequireColor(string value, string field, List<string> missing)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            missing.Add(field);
            return;
        }

        if (!ColorUtility.TryParseHtmlString(value, out _))
            missing.Add($"{field} has invalid color value: {value}");
    }

    private static void RequireFont(
        ThemeFontLibrary fontLibrary,
        string key,
        string field,
        List<string> missing)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        if (!fontLibrary.HasFont(key))
            missing.Add($"{field} references missing ThemeFontLibrary key: {key}");
    }
}