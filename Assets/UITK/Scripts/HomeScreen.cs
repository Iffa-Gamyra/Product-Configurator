using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class HomeScreen : MonoBehaviour
{
    [Header("Camera / World")]
    public CameraController cameraController;
    public DecalController decalController;

    [Header("Targets")]
    public Transform rotationTarget;
    public Transform spawnPoint;

    [Header("Fixed Camera Anchors")]
    public Transform swoopPosition;
    public Transform startPosition;
    public Transform productViewPosition;
    public Transform videoPosition;

    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("Products")]
    [SerializeField] private string resourcesPath = "GamyraDrive";
    [SerializeField] private string defaultProductId = "B1";
    [SerializeField] private string currentProductId = "B1";

    [Header("UI Row Templates")]
    public VisualTreeAsset productButtonTemplate;
    public VisualTreeAsset specRowTextTemplate;
    public VisualTreeAsset specRowBarTemplate;
    public VisualTreeAsset specRowToggleTemplate;
    public VisualTreeAsset specRowChipsTemplate;
    public VisualTreeAsset inspectRowTemplate;

    [Header("UI Themes")]
    [SerializeField] private ConfiguratorUITheme desktopTheme;
    [SerializeField] private ConfiguratorUITheme mobileTheme;

    [Header("Video")]
    public float video_FOV;
    public float normal_FOV;

    // Camera indices
    private const int CAM_SWOOP = 0;
    private const int CAM_START = 1;
    private const int CAM_PRODUCT_VIEW = 2;
    private const int CAM_VIDEO = 3;
    private const int FIRST_DYNAMIC_CAM = 4;

    // State
    private bool userRotated = false;
    private bool uiHidden = false;
    private bool uiInitialized = false;
    private bool isMobileLayout = false;

    // UI
    private VisualElement root;
    private HomeScreenUI ui;

    // Products
    private ProductManager productManager;
    private Product currentProduct;

    // Modules
    private ScreenNavigator nav;
    private VideoUIController videoUI;
    private SpecsUIController specsUI;
    private InspectUIController inspectUI;
    private ProductSelectionUIController productSelectionUI;
    private CameraRigBuilder rigBuilder;
    private HomeScreenDisplayFlow displayFlow;
    private HomeSceneModeController sceneMode;
    private UIThemeApplicator themeApplicator;

    // Theme
    private ConfiguratorUITheme currentTheme;

    // Video
    private VideoPlayer videoPlayer;
    private Camera mainCam;

    private Dictionary<string, Action> actions;

    private enum NavMode { Home, Product, Video }

    private void Awake()
    {
        mainCam = Camera.main;
        videoPlayer = GetComponent<VideoPlayer>();

        productManager = new ProductManager(resourcesPath, spawnPoint);
        rigBuilder = new CameraRigBuilder(
            cameraController, swoopPosition, startPosition,
            productViewPosition, videoPosition);

        BuildActions();
        rigBuilder.ApplyBaseRig(FIRST_DYNAMIC_CAM);
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(currentProductId))
            currentProductId = defaultProductId;

        if (!ProductExists(currentProductId))
            currentProductId = GetFirstProductIdOrDefault(currentProductId);

        SelectProduct(currentProductId);
    }

    private void OnEnable()
    {
        if (uiDocument == null) return;
        root = uiDocument.rootVisualElement;
        if (root == null) return;

        isMobileLayout = DeviceDetection.IsMobileActive;
        ui = new HomeScreenUI(root, isMobileLayout);
        currentTheme = isMobileLayout ? mobileTheme : desktopTheme;

        themeApplicator = new UIThemeApplicator(ui, isMobileLayout);
        themeApplicator.Apply(currentTheme);

        nav = new ScreenNavigator(ui.AllScreens, ui.InfoOverlay);
        displayFlow = new HomeScreenDisplayFlow(ui, isMobileLayout, nav);

        sceneMode = new HomeSceneModeController(
            mainCam, cameraController, decalController, rotationTarget,
            nav, IsAnyProductScreenVisible, video_FOV, normal_FOV);

        if (videoUI == null)
        {
            videoUI = new VideoUIController(
                this, videoPlayer,
                ui.PlayButton, ui.PauseButton, ui.MuteButton,
                ui.UnmuteButton, ui.ReplayButton, ui.ProgressBar,
                () => ScreenNavigator.IsVisible(ui.VideoScreen));
        }

        if (specsUI == null)
        {
            specsUI = new SpecsUIController(
                specRowTextTemplate, specRowBarTemplate,
                specRowToggleTemplate, specRowChipsTemplate);
        }

        productSelectionUI = new ProductSelectionUIController(
            ui.ProductsContainer,
            productButtonTemplate,
            ui.SelectedProductInSpecScreen,
            ui.SelectedProductInInspectScreen
        );

        inspectUI = new InspectUIController(
            cameraController, CAM_PRODUCT_VIEW, FIRST_DYNAMIC_CAM,
            ui.InspectListContainer, inspectRowTemplate,
            ui.ResetViewButton, ui.InspectPrevButton, ui.InspectNextButton,
            currentTheme?.images?.iconFocus);

        if (!uiInitialized)
        {
            ui.BindButtons(actions);
            ui.BindInfoOverlayButtons(ToggleInfoOverlay);

            productSelectionUI.BuildIfNeeded(productManager.LoadedProducts, SelectProduct);
            productSelectionUI.UpdateSelected(currentProductId, currentProduct);

            if (ui.WelcomeScreen != null)
                BindWelcomeScreen();

            uiInitialized = true;
        }
        else
        {
            productSelectionUI.BuildIfNeeded(productManager.LoadedProducts, SelectProduct);
            productSelectionUI.UpdateSelected(currentProductId, currentProduct);
        }

        videoUI.Hook();
        SyncNavToVisibleScreen();
    }

    private void OnDisable()
    {
        videoUI?.Unhook();
    }

    private void BindWelcomeScreen()
    {
        var wsManager = new WelcomeScreenManager(ui.WelcomeStartBtn);
        wsManager.BindStart(() =>
        {
            if (isMobileLayout)
            {
                cameraController?.SetTarget(rotationTarget, true);
                cameraController?.goToPosition(CAM_PRODUCT_VIEW);
            }
            else
            {
                cameraController?.goToPosition(CAM_START);
            }

            displayFlow.ShowHome();
            sceneMode.SetVideoFov(false);
            SyncNavToVisibleScreen();
            sceneMode.RefreshRotationState();

            if (videoPlayer != null && !videoPlayer.isPrepared)
                videoPlayer.Prepare();
        });
    }


    private void BuildActions()
    {
        actions = new Dictionary<string, Action>
        {
            // Desktop SideNav
            [UINames.SideNav_Home] = GoHome,
            [UINames.SideNav_Product] = OpenProductRoot,
            [UINames.SideNav_Video] = OpenVideo,

            // Desktop TopNav
            [UINames.TopNav_Product] = () => OpenProductPage(0),
            [UINames.TopNav_Specs] = () => OpenProductPage(1),
            [UINames.TopNav_Inspect] = () => OpenProductPage(2),

            // Desktop Home buttons
            [UINames.Home_ProductTabBtn] = OpenProductRoot,
            [UINames.Home_VideoTabBtn] = OpenVideo,

            // Desktop panel buttons
            [UINames.Products_NextBtn] = OpenNextFromProduct,
            [UINames.Specs_NextBtn] = OpenNextFromSpecs,
            [UINames.Specs_BackNavBtn] = () => OpenProductPage(0),
            [UINames.Inspect_BackNavBtn] = OpenPrevFromInspect,
            [UINames.Inspect_DoneBtn] = GoHome,

            // Inspect controls
            [UINames.Inspect_ResetBtn] = () => inspectUI?.ResetView(),
            [UINames.Inspect_PrevBtn] = () => inspectUI?.InspectPrev(),
            [UINames.Inspect_NextBtn] = () => inspectUI?.InspectNext(),

            // Utils
            [UINames.Utils_Hide] = ToggleUIVisibility,
            [UINames.Utils_Screenshot] = TakeScreenshot,

            // Brochure
            [UINames.Specs_DownloadBtn] = DownloadPdf,

            // Mobile TopNav
            [UINames.MobileTopNav_Home] = GoHome,
            [UINames.MobileTopNav_Video] = OpenVideo,

            // Video controls
            [UINames.Video_PlayBtn] = () => videoUI?.Play(),
            [UINames.Video_PauseBtn] = () => videoUI?.Pause(),
            [UINames.Video_MuteBtn] = () => videoUI?.Mute(),
            [UINames.Video_UnmuteBtn] = () => videoUI?.Unmute(),
            [UINames.Video_ReplayBtn] = () => videoUI?.Replay(),
        };
    }


    private void SetActiveNav(NavMode mode)
    {
        if (ui == null) return;

        if (isMobileLayout)
        {
            bool videoActive = mode == NavMode.Video;
            foreach (var b in ui.MobileHomeTabBtns) SetTabState(b, !videoActive);
            foreach (var b in ui.MobileVideoTabBtns) SetTabState(b, videoActive);
            return;
        }

        foreach (var b in ui.SideNavHomeBtns) SetNavIcon(b, mode == NavMode.Home);
        foreach (var b in ui.SideNavProductBtns) SetNavIcon(b, mode == NavMode.Product);
        foreach (var b in ui.SideNavVideoBtns) SetNavIcon(b, mode == NavMode.Video);


        if (mode != NavMode.Product)
        {

            foreach (var b in ui.SpecsTabButtons)
            {
                b.EnableInClassList(UINames.Class_TabActive, false);
                b.EnableInClassList(UINames.Class_TabInactive, true);
                b.style.color = currentTheme?.colors.secondaryText ?? Color.gray;
            }
            foreach (var b in ui.InspectTabButtons)
            {
                b.EnableInClassList(UINames.Class_TabActive, false);
                b.EnableInClassList(UINames.Class_TabInactive, true);
                b.style.color = currentTheme?.colors.secondaryText ?? Color.gray;
            }
            return;
        }

        bool onProducts = ScreenNavigator.IsVisible(ui.ProductSelectionScreen);
        bool onSpecs = ScreenNavigator.IsVisible(ui.ProductSpecsScreen);
        bool onInspect = ScreenNavigator.IsVisible(ui.InspectProductScreen);


        root.Query<Button>(UINames.TopNav_Product).ForEach(b =>
        {
            bool active = onProducts;
            b.EnableInClassList(UINames.Class_TabActive, active);
            b.EnableInClassList(UINames.Class_TabInactive, !active);
            b.style.color = active
                ? (currentTheme?.colors.accentPrimary ?? Color.white)
                : (currentTheme?.colors.secondaryText ?? Color.gray);
        });

        foreach (var b in ui.SpecsTabButtons)
        {
            bool active = onSpecs;
            b.EnableInClassList(UINames.Class_TabActive, active);
            b.EnableInClassList(UINames.Class_TabInactive, !active);
            b.style.color = active
                ? (currentTheme?.colors.accentPrimary ?? Color.white)
                : (currentTheme?.colors.secondaryText ?? Color.gray);
        }

        foreach (var b in ui.InspectTabButtons)
        {
            bool active = onInspect;
            b.EnableInClassList(UINames.Class_TabActive, active);
            b.EnableInClassList(UINames.Class_TabInactive, !active);
            b.style.color = active
                ? (currentTheme?.colors.accentPrimary ?? Color.white)
                : (currentTheme?.colors.secondaryText ?? Color.gray);
        }
    }

    private void SetNavIcon(Button btn, bool active)
    {
        if (btn == null) return;
        btn.EnableInClassList(UINames.Class_NavIconActive, active);
        btn.style.backgroundColor = active
            ? (currentTheme?.colors.navIconActiveBg ?? Color.white)
            : (currentTheme?.colors.navIconInactiveBg ?? Color.grey);
    }

    private void SetTabState(Button btn, bool active)
    {
        if (btn == null) return;
        btn.EnableInClassList(UINames.Class_TabActive, active);
        btn.EnableInClassList(UINames.Class_TabInactive, !active);
        btn.style.color = active
            ? (currentTheme?.colors.accentPrimary ?? Color.white)
            : (currentTheme?.colors.secondaryText ?? Color.gray);
    }

    private void SyncNavToVisibleScreen()
    {
        if (ui == null) return;

        if (isMobileLayout)
        {
            SetActiveNav(ScreenNavigator.IsVisible(ui.VideoScreen)
                ? NavMode.Video : NavMode.Home);
            return;
        }

        if (ScreenNavigator.IsVisible(ui.VideoScreen))
        { SetActiveNav(NavMode.Video); return; }

        if (ScreenNavigator.IsVisible(ui.ProductSelectionScreen) ||
            ScreenNavigator.IsVisible(ui.ProductSpecsScreen) ||
            ScreenNavigator.IsVisible(ui.InspectProductScreen))
        { SetActiveNav(NavMode.Product); return; }

        SetActiveNav(NavMode.Home);
    }

    private void GoHome()
    {
        rigBuilder.ApplyBaseRig(FIRST_DYNAMIC_CAM);
        cameraController?.goToPosition(CAM_START);
        videoUI?.Pause();
        videoUI?.Leave();
        sceneMode.SetVideoFov(false);
        sceneMode.SetProductDecalVisible(false);

        if (isMobileLayout)
        {
            cameraController?.SetTarget(rotationTarget, true);
            cameraController?.goToPosition(CAM_PRODUCT_VIEW);
        }

        displayFlow.ShowHome();
        SyncNavToVisibleScreen();
        sceneMode.RefreshRotationState();
    }

    private void OpenVideo()
    {
        rigBuilder.ApplyBaseRig(FIRST_DYNAMIC_CAM);
        sceneMode.SetVideoFov(true);
        cameraController?.goToPosition(CAM_VIDEO);
        sceneMode.SetProductDecalVisible(false);
        displayFlow.ShowVideo();
        videoUI?.Enter();
        SyncNavToVisibleScreen();
        sceneMode.RefreshRotationState();
    }

    private void OpenProductRoot()
    {
        videoUI?.Pause();
        videoUI?.Leave();
        sceneMode.SetVideoFov(false);
        rigBuilder.ApplyProductRig(currentProduct, FIRST_DYNAMIC_CAM);
        cameraController?.goToPosition(CAM_PRODUCT_VIEW);
        cameraController?.SetTarget(rotationTarget, true);
        sceneMode.SetProductDecalVisible(true);
        OpenProductSelection();
    }

    private void OpenProductSelection()
    {
        displayFlow.ShowProductSelection();
        if (!isMobileLayout)
        {
            productSelectionUI?.BuildIfNeeded(productManager.LoadedProducts, SelectProduct);
            productSelectionUI?.UpdateSelected(currentProductId, currentProduct);
        }
        SyncNavToVisibleScreen();
        sceneMode.RefreshRotationState();
    }

    private void OpenSpecs()
    {
        if (isMobileLayout)
        {
            specsUI?.PopulateSpecs(ui.SpecsListContainer, currentProduct);
            UpdateBrochureButtonState();
            return;
        }
        displayFlow.ShowSpecs();
        specsUI?.PopulateSpecs(ui.SpecsListContainer, currentProduct);
        UpdateBrochureButtonState();
        productSelectionUI?.UpdateSelected(currentProductId, currentProduct);
        SyncNavToVisibleScreen();
        sceneMode.RefreshRotationState();
    }

    private void OpenInspect()
    {
        if (isMobileLayout)
        {
            inspectUI?.Rebuild(currentProduct);
            return;
        }
        displayFlow.ShowInspect();
        inspectUI?.Rebuild(currentProduct);
        productSelectionUI?.UpdateSelected(currentProductId, currentProduct);
        SyncNavToVisibleScreen();
        sceneMode.RefreshRotationState();
    }

    private void OpenProductPage(int pageIndex)
    {
        nav?.CloseOverlay();
        sceneMode.SetProductDecalVisible(true);
        switch (pageIndex)
        {
            case 0: OpenProductSelection(); break;
            case 1: OpenSpecs(); break;
            case 2: OpenInspect(); break;
        }
    }

    private void OpenNextFromProduct()
    {
        nav?.CloseOverlay();
        sceneMode.SetProductDecalVisible(true);
        if (HasSpecs(currentProduct)) OpenSpecs();
        else if (HasInspectPoints(currentProduct)) OpenInspect();
    }

    private void OpenNextFromSpecs()
    {
        nav?.CloseOverlay();
        if (HasInspectPoints(currentProduct)) OpenInspect();
        else GoHome();
    }

    private void OpenPrevFromInspect()
    {
        nav?.CloseOverlay();
        sceneMode.SetProductDecalVisible(true);
        if (HasSpecs(currentProduct)) OpenSpecs();
        else OpenProductSelection();
    }

    private void ToggleInfoOverlay()
    {
        nav?.ToggleOverlay();
        sceneMode.RefreshRotationState();
    }


    private void RefreshProductDependentUI()
    {
        bool hasSpecs = HasSpecs(currentProduct);
        bool hasInspect = HasInspectPoints(currentProduct);
        bool hasBrochure = HasBrochure(currentProduct);

        SetDisplay(ui.SpecsButton, hasSpecs || hasInspect);

        if (ui.InspectButton != null)
            ui.InspectButton.text = hasInspect
                ? (currentTheme?.text.inspectButton ?? "NEXT")
                : (currentTheme?.text.doneButton ?? "DONE");

        foreach (var b in ui.SpecsTabButtons)
        {
            b.SetEnabled(hasSpecs);
            b.EnableInClassList(UINames.Class_TabDisabled, !hasSpecs);
        }
        foreach (var b in ui.InspectTabButtons)
        {
            b.SetEnabled(hasInspect);
            b.EnableInClassList(UINames.Class_TabDisabled, !hasInspect);
        }


        SetDisplay(ui.SpecsSectionRoot, hasSpecs);
        SetDisplay(ui.InspectSectionRoot, hasInspect);
        SetDisplay(ui.BrochureSectionRoot, hasBrochure);
        SetDisplay(ui.DownloadPdfButton, hasBrochure);

        if (ui.InspectBackNavLabel != null)
            ui.InspectBackNavLabel.text = hasSpecs
                ? (currentTheme?.text.specsSectionTitle ?? "SPECS")
                : (currentTheme?.text.topTabProduct ?? "CHOOSE PRODUCT");

        if (hasSpecs) specsUI?.PopulateSpecs(ui.SpecsListContainer, currentProduct);
        else ui.SpecsListContainer?.Clear();

        if (hasInspect) inspectUI?.Rebuild(currentProduct);
        else
        {
            ui.InspectListContainer?.Clear();
            inspectUI?.ResetView();
        }

        UpdateBrochureButtonState();
    }

    private static void SetDisplay(VisualElement el, bool visible)
    {
        if (el != null)
            el.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }


    private void SelectProduct(string id)
    {
        if (currentProductId == id && currentProduct != null)
        {
            productSelectionUI?.UpdateSelected(currentProductId, currentProduct);
            return;
        }

        currentProductId = id;

        if (!productManager.Select(id))
        {
            currentProduct = null;
            productSelectionUI?.UpdateSelected(currentProductId, currentProduct);
            UpdateBrochureButtonState();
            return;
        }

        currentProduct = productManager.CurrentProduct;
        rigBuilder.ApplyProductRig(currentProduct, FIRST_DYNAMIC_CAM);

        if (IsAnyProductScreenVisible() && userRotated)
        {
            cameraController?.goToPosition(CAM_PRODUCT_VIEW);
            inspectUI?.ResetView();
            userRotated = false;
        }

        productSelectionUI?.UpdateSelected(currentProductId, currentProduct);
        RefreshProductDependentUI();
        SyncNavToVisibleScreen();
    }

    private bool IsAnyProductScreenVisible()
    {
        if (isMobileLayout) return ScreenNavigator.IsVisible(ui.HomeScreen);
        return ScreenNavigator.IsVisible(ui.ProductSelectionScreen) ||
               ScreenNavigator.IsVisible(ui.ProductSpecsScreen) ||
               ScreenNavigator.IsVisible(ui.InspectProductScreen);
    }

    private void ToggleUIVisibility()
    {
        uiHidden = !uiHidden;
        if (!isMobileLayout)
        {
            var display = uiHidden ? DisplayStyle.None : DisplayStyle.Flex;
            foreach (var e in ui.RightContainers)
                e.style.display = display;
        }
        else
        {
            if (ui.BottomPanel != null)
                ui.BottomPanel.style.display = uiHidden ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }

    private void TakeScreenshot() =>
        ScreenshotHandler.Instance?.CaptureScreenshot();

    private void UpdateBrochureButtonState()
    {
        bool hasPdf = HasBrochure(currentProduct);
        if (ui.DownloadPdfButton != null)
        {
            ui.DownloadPdfButton.style.display = hasPdf ? DisplayStyle.Flex : DisplayStyle.None;
            ui.DownloadPdfButton.SetEnabled(hasPdf);
        }
        if (ui.BrochureSectionRoot != null)
            ui.BrochureSectionRoot.style.display = hasPdf ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void DownloadPdf()
    {
        if (currentProduct == null) return;
        WebGLDownloadManager.Instance?.DownloadPdfFromServer(
            currentProduct.brochurePdfFile,
            currentProduct.brochureDownloadName);
    }

    public void NotifyUserRotated() => userRotated = true;

    private bool HasSpecs(Product p)
    {
        if (p == null || !p.showSpecs || p.specs == null || p.specs.Length == 0) return false;
        foreach (var s in p.specs)
            if (s != null && (!string.IsNullOrWhiteSpace(s.label) || !string.IsNullOrWhiteSpace(s.value)))
                return true;
        return false;
    }

    private bool HasInspectPoints(Product p)
    {
        if (p == null || !p.showInspect || p.inspectPoints == null || p.inspectPoints.Length == 0) return false;
        foreach (var pt in p.inspectPoints)
            if (pt != null && (!string.IsNullOrWhiteSpace(pt.label) || pt.cameraAnchor != null))
                return true;
        return false;
    }

    private bool HasBrochure(Product p) =>
        p != null && !string.IsNullOrWhiteSpace(p.brochurePdfFile);

    private bool ProductExists(string id)
    {
        if (string.IsNullOrEmpty(id) || productManager == null) return false;
        foreach (var p in productManager.LoadedProducts)
            if (p != null && p.productId == id) return true;
        return false;
    }

    private string GetFirstProductIdOrDefault(string fallback)
    {
        var list = productManager?.LoadedProducts;
        if (list != null && list.Count > 0 && list[0] != null &&
            !string.IsNullOrEmpty(list[0].productId))
            return list[0].productId;
        return fallback;
    }
}
