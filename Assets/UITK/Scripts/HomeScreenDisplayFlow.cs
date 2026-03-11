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
        if (isMobileLayout)
            ShowMobileHome();
        else
            nav?.ShowOnly(ui.HomeScreen);
    }

    public void ShowVideo()
    {
        if (isMobileLayout)
        {
            if (ui.HomeScreen != null)
                ui.HomeScreen.style.display = DisplayStyle.None;

            if (ui.VideoScreen != null)
                ui.VideoScreen.style.display = DisplayStyle.Flex;
        }
        else
        {
            nav?.ShowOnly(ui.VideoScreen);
        }
    }

    public void ShowModelSelection()
    {
        if (isMobileLayout)
        {
            if (ui.HomeScreen != null)
                ui.HomeScreen.style.display = DisplayStyle.Flex;
        }
        else
        {
            nav?.ShowOnly(ui.ModelSelectionScreen);
        }
    }

    public void ShowSpecs()
    {
        if (!isMobileLayout)
            nav?.ShowOnly(ui.ModelSpecsScreen);
    }

    public void ShowInspect()
    {
        if (!isMobileLayout)
            nav?.ShowOnly(ui.InspectModelScreen);
    }

    private void ShowMobileHome()
    {
        if (ui.WelcomeScreen != null)
            ui.WelcomeScreen.style.display = DisplayStyle.None;

        if (ui.HomeScreen != null)
            ui.HomeScreen.style.display = DisplayStyle.Flex;

        if (ui.VideoScreen != null)
            ui.VideoScreen.style.display = DisplayStyle.None;

        if (ui.ModelSelectionScreen != null)
            ui.ModelSelectionScreen.style.display = DisplayStyle.Flex;

        if (ui.ModelSpecsScreen != null)
            ui.ModelSpecsScreen.style.display = DisplayStyle.Flex;

        if (ui.InspectModelScreen != null)
            ui.InspectModelScreen.style.display = DisplayStyle.Flex;
    }
}