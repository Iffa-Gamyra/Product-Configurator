using System.Collections.Generic;
using UnityEngine.UIElements;

public sealed class ScreenNavigator
{
    private readonly List<VisualElement> allScreens;
    private readonly VisualElement infoOverlay;

    public VisualElement CurrentScreen { get; private set; }

    public ScreenNavigator(List<VisualElement> allScreens, VisualElement infoOverlay)
    {
        this.allScreens = allScreens ?? new List<VisualElement>();
        this.infoOverlay = infoOverlay;
    }

    public void ShowOnly(VisualElement screen)
    {
        if (screen == null) return;
        CloseOverlay();
        for (int i = 0; i < allScreens.Count; i++)
            SetVisible(allScreens[i], false);
        SetVisible(screen, true);
        CurrentScreen = screen;
    }

    public void SetCurrent(VisualElement screen)
    {
        CurrentScreen = screen;
    }

    public void ToggleOverlay()
    {
        if (infoOverlay == null) return;
        bool isOpen = infoOverlay.style.display == DisplayStyle.Flex;
        infoOverlay.style.display = isOpen ? DisplayStyle.None : DisplayStyle.Flex;
        if (!isOpen) infoOverlay.BringToFront();
    }

    public void CloseOverlay()
    {
        if (infoOverlay != null)
            infoOverlay.style.display = DisplayStyle.None;
    }

    public bool IsOverlayOpen()
        => infoOverlay != null && infoOverlay.style.display != DisplayStyle.None;

    public static bool IsVisible(VisualElement ve)
        => ve != null && ve.style.display != DisplayStyle.None;

    private static void SetVisible(VisualElement ve, bool visible)
    {
        if (ve == null) return;
        ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
