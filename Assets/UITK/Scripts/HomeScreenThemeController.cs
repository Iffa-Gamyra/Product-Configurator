using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HomeScreenThemeController
{
    private readonly MonoBehaviour host;
    private readonly UIDocument uiDocument;
    private readonly HomeScreenThemeBootstrap themeBootstrap;

    private Coroutine bootRoutine;
    private Action<VisualElement, HomeScreenUI, RuntimeThemeData, bool> onComplete;

    private HomeScreenUI ui;
    private bool isMobileLayout;

    private VisualElement loadingPanel;
    private VisualElement loadingBarFill;
    private VisualElement errorPanel;
    private Label errorBodyLabel;

    // Stored so ShowError can be called after retry from HomeScreen
    private string lastError;

    public HomeScreenThemeController(
        MonoBehaviour host,
        UIDocument uiDocument,
        HomeScreenThemeBootstrap themeBootstrap)
    {
        this.host = host;
        this.uiDocument = uiDocument;
        this.themeBootstrap = themeBootstrap;
    }

    public void StartBoot(Action<VisualElement, HomeScreenUI, RuntimeThemeData, bool> callback)
    {
        onComplete = callback;

        if (host == null)
        {
            Debug.LogError("HomeScreenThemeController: host is null.");
            return;
        }

        StopBoot();
        bootRoutine = host.StartCoroutine(Bootstrap());
    }

    public void StopBoot()
    {
        if (host != null && bootRoutine != null)
        {
            host.StopCoroutine(bootRoutine);
            bootRoutine = null;
        }
    }

    public void Retry()
    {
        StartBoot(onComplete);
    }

    // Called by HomeScreen after full init to show error panel
    public void ShowErrorPanel(string message = null)
    {
        if (errorPanel == null) return;

        errorPanel.style.display = DisplayStyle.Flex;
        errorPanel.pickingMode = PickingMode.Position;

        if (errorBodyLabel != null)
            errorBodyLabel.text = string.IsNullOrWhiteSpace(message)
                ? "Theme load failed."
                : message;
    }

    public void HideErrorPanel()
    {
        if (errorPanel == null) return;
        errorPanel.style.display = DisplayStyle.None;
        errorPanel.pickingMode = PickingMode.Ignore;
    }

    public void HideLoadingPanel()
    {
        if (loadingPanel == null) return;
        loadingPanel.style.display = DisplayStyle.None;
        loadingPanel.pickingMode = PickingMode.Ignore;
    }

    public string LastError => lastError;

    private IEnumerator Bootstrap()
    {
        yield return null;
        yield return null;

        if (uiDocument == null)
        {
            Debug.LogError("HomeScreenThemeController: UIDocument is null.");
            yield break;
        }

        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("HomeScreenThemeController: rootVisualElement is null.");
            yield break;
        }

        isMobileLayout = DeviceDetection.IsMobileActive;
        ui = new HomeScreenUI(root, isMobileLayout);

        // Cache panel refs
        loadingPanel = ui.LoadingScreen;
        loadingBarFill = ui.LoadingBarFill;
        errorPanel = ui.ErrorScreen;
        errorBodyLabel = ui.ErrorBodyLabel;

        // Start with loading visible, error hidden
        HideErrorPanel();
        ShowLoading(true);
        SetProgress(0f);

        // Try to load theme
        RuntimeThemeData loadedTheme = null;
        string themeError = null;

        if (themeBootstrap == null)
        {
            themeError = "HomeScreenThemeBootstrap is not assigned.";
        }
        else
        {
            yield return host.StartCoroutine(themeBootstrap.LoadThemeForCurrentDevice(
                isMobileLayout,
                theme => loadedTheme = theme,
                error => themeError = error,
                progress => SetProgress(progress)));
        }

        // Store error for HomeScreen to use
        lastError = themeError;

        // Always complete progress and hide loading
        SetProgress(1f);
        yield return null;
        ShowLoading(false);

        // Always call onComplete  HomeScreen decides what to show
        onComplete?.Invoke(root, ui, loadedTheme, isMobileLayout);
    }

    private void ShowLoading(bool visible)
    {
        if (loadingPanel == null) return;
        loadingPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        loadingPanel.pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;
    }

    private void SetProgress(float t)
    {
        if (loadingBarFill == null) return;
        loadingBarFill.style.width = Length.Percent(t * 100f);
    }
}
