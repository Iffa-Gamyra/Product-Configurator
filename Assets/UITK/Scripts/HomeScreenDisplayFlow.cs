using UnityEngine.UIElements;

public class HomeScreenDisplayFlow
{
    private readonly HomeScreenUI ui;
    private readonly bool isMobileLayout;
    private readonly ScreenNavigator nav;

    public HomeScreenDisplayFlow(HomeScreenUI ui, bool isMobileLayout, ScreenNavigator nav)
    {
        this.ui = ui;
        this.isMobileLayout = isMobileLayout;
        this.nav = nav;
    }

    public void ShowHome()
    {
        if (isMobileLayout) ShowMobileHome();
        else nav?.ShowOnly(ui.HomeScreen);
    }

    public void ShowVideo()
    {
        if (isMobileLayout)
        {
            if (ui.HomeScreen != null) ui.HomeScreen.style.display = DisplayStyle.None;
            if (ui.VideoScreen != null) ui.VideoScreen.style.display = DisplayStyle.Flex;
            nav?.SetCurrent(ui.VideoScreen);
        }
        else
        {
            nav?.ShowOnly(ui.VideoScreen);
        }
    }

    public void ShowProductSelection()
    {
        if (isMobileLayout)
        {
            if (ui.HomeScreen != null) ui.HomeScreen.style.display = DisplayStyle.Flex;
            nav?.SetCurrent(ui.HomeScreen);
        }
        else
        {
            nav?.ShowOnly(ui.ProductSelectionScreen);
        }
    }

    public void ShowSpecs()
    {
        if (!isMobileLayout) nav?.ShowOnly(ui.ProductSpecsScreen);
    }

    public void ShowInspect()
    {
        if (!isMobileLayout) nav?.ShowOnly(ui.InspectProductScreen);
    }

    private void ShowMobileHome()
    {
        if (ui.WelcomeScreen != null) ui.WelcomeScreen.style.display = DisplayStyle.None;
        if (ui.VideoScreen != null) ui.VideoScreen.style.display = DisplayStyle.None;
        if (ui.HomeScreen != null) ui.HomeScreen.style.display = DisplayStyle.Flex;
        if (ui.ProductSelectionScreen != null) ui.ProductSelectionScreen.style.display = DisplayStyle.Flex;
        if (ui.ProductSpecsScreen != null) ui.ProductSpecsScreen.style.display = DisplayStyle.Flex;
        if (ui.InspectProductScreen != null) ui.InspectProductScreen.style.display = DisplayStyle.Flex;
        nav?.SetCurrent(ui.HomeScreen);
    }
}
