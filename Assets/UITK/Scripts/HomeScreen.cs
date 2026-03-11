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
    public Transform modelViewPosition;
    public Transform videoPosition;

    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("GamyraDrive Models")]
    [SerializeField] private string resourcesPath = "GamyraDrive";
    [SerializeField] private string defaultModelId = "B1";
    [SerializeField] private string currentModelId = "B1";

    [Header("UI Templates")]
    public VisualTreeAsset modelButtonTemplate;
    public VisualTreeAsset specRowTextTemplate;
    public VisualTreeAsset specRowBarTemplate;
    public VisualTreeAsset specRowToggleTemplate;
    public VisualTreeAsset specRowChipsTemplate;
    public VisualTreeAsset inspectRowTemplate;

    [Header("Video")]
    public float video_FOV;
    public float normal_FOV;

    // Camera indices
    private const int CAM_SWOOP = 0;
    private const int CAM_START = 1;
    private const int CAM_MODEL_VIEW = 2;
    private const int CAM_VIDEO = 3;
    private const int FIRST_DYNAMIC_CAM = 4;

    private bool userRotated = false;

    // UI
    private VisualElement root;
    private HomeScreenUI ui;

    // State
    private bool uiHidden = false;
    private bool uiInitialized = false;
    private bool isMobileLayout = false;

    // Models
    private ModelManager modelManager;
    private SimulatorModel currentSimModel;

    // Modules
    private ScreenNavigator nav;
    private VideoUIController videoUI;
    private SpecsUIController specsUI;
    private InspectUIController inspectUI;
    private ModelSelectionUIController modelSelectionUI;
    private CameraRigBuilder rigBuilder;

    // Homescreen helpers
    private HomeScreenDisplayFlow displayFlow;
    private HomeSceneModeController sceneMode;

    // Video + camera
    private VideoPlayer videoPlayer;
    private Camera mainCam;

    private Dictionary<string, Action> actions;

    private void Awake()
    {
        mainCam = Camera.main;
        videoPlayer = GetComponent<VideoPlayer>();

        modelManager = new ModelManager(resourcesPath, spawnPoint);
        rigBuilder = new CameraRigBuilder(cameraController, swoopPosition, startPosition, modelViewPosition, videoPosition);

        BuildActions();
        rigBuilder.ApplyBaseRig(FIRST_DYNAMIC_CAM);
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(currentModelId))
            currentModelId = defaultModelId;

        if (!ModelExists(currentModelId))
            currentModelId = GetFirstModelIdOrDefault(currentModelId);

        SelectSimModel(currentModelId);
    }

    private void OnEnable()
    {
        if (uiDocument == null) return;

        root = uiDocument.rootVisualElement;
        if (root == null) return;

        isMobileLayout = DeviceDetection.IsMobileActive;
        ui = new HomeScreenUI(root, isMobileLayout);

        nav = new ScreenNavigator(ui.AllScreens, ui.InfoOverlay);

        displayFlow = new HomeScreenDisplayFlow(ui, isMobileLayout, nav);

        sceneMode = new HomeSceneModeController(
            mainCam,
            cameraController,
            decalController,
            rotationTarget,
            nav,
            IsAnyModelScreenVisible,
            video_FOV,
            normal_FOV
        );

        if (videoUI == null)
        {
            videoUI = new VideoUIController(
                this,
                videoPlayer,
                ui.PlayButton,
                ui.PauseButton,
                ui.MuteButton,
                ui.UnmuteButton,
                ui.ReplayButton,
                ui.ProgressBar,
                () => ScreenNavigator.IsVisible(ui.VideoScreen)
            );
        }

        if (specsUI == null)
        {
            specsUI = new SpecsUIController(
                specRowTextTemplate,
                specRowBarTemplate,
                specRowToggleTemplate,
                specRowChipsTemplate
            );
        }

        modelSelectionUI = new ModelSelectionUIController(
            ui.ModelsContainer,
            modelButtonTemplate,
            ui.SelectedModelInSpecScreen,
            ui.SelectedModelInInspectScreen
        );

        inspectUI = new InspectUIController(
            cameraController,
            CAM_MODEL_VIEW,
            FIRST_DYNAMIC_CAM,
            ui.InspectListContainer,
            inspectRowTemplate,
            ui.ResetViewButton,
            ui.InspectPrevButton,
            ui.InspectNextButton
        );

        if (!uiInitialized)
        {
            ui.BindButtons(actions);
            ui.BindInfoOverlayButtons(ToggleInfoOverlay);

            modelSelectionUI.BuildIfNeeded(modelManager.LoadedModels, SelectSimModel);
            modelSelectionUI.UpdateSelected(currentModelId, currentSimModel);

            if (ui.WelcomeScreen != null)
                BindWelcomeScreen();

            uiInitialized = true;
        }
        else
        {
            modelSelectionUI.BuildIfNeeded(modelManager.LoadedModels, SelectSimModel);
            modelSelectionUI.UpdateSelected(currentModelId, currentSimModel);
        }

        videoUI.Hook();
    }

    private void OnDisable()
    {
        videoUI?.Unhook();
    }

    // -------------------------
    // Actions / binding
    // -------------------------
    private void BindWelcomeScreen()
    {
        var wsManager = new WelcomeScreenManager(ui.WelcomeScreen);

        wsManager.BindStart(() =>
        {
            if (isMobileLayout)
            {
                if (cameraController != null)
                {
                    cameraController.SetTarget(rotationTarget, true);
                    cameraController.goToPosition(CAM_MODEL_VIEW);
                }
            }
            else
            {
                if (cameraController != null)
                    cameraController.goToPosition(CAM_START);
            }

            displayFlow.ShowHome();
            sceneMode.SetVideoFov(false);
            sceneMode.RefreshRotationState();

            if (videoPlayer != null && !videoPlayer.isPrepared)
                videoPlayer.Prepare();
        });
    }

    private void BuildActions()
    {
        actions = new Dictionary<string, Action>
        {
            ["homeModeButton"] = GoHome,

            ["videoModeButton"] = OpenVideo,
            ["videoTabButton"] = OpenVideo,

            ["modelModeButton"] = OpenModelRoot,
            ["modelTabButton"] = OpenModelRoot,

            ["play-button"] = () => videoUI?.Play(),
            ["pause-button"] = () => videoUI?.Pause(),
            ["mute-button"] = () => videoUI?.Mute(),
            ["unmute-button"] = () => videoUI?.Unmute(),
            ["replay-button"] = () => videoUI?.Replay(),

            ["modelTCButton"] = () => OpenModelPage(0),
            ["specsTCButton"] = () => OpenModelPage(1),
            ["inspectTCButton"] = () => OpenModelPage(2),

            ["specsButton"] = () => OpenModelPage(1),
            ["inspectButton"] = () => OpenModelPage(2),
            ["viewModelsButton"] = () => OpenModelPage(0),
            ["viewSpecsButton"] = () => OpenModelPage(1),

            ["downloadButton"] = DownloadPdf,

            ["resetViewButton"] = () => inspectUI?.ResetView(),
            ["inspectPrevButton"] = () => inspectUI?.InspectPrev(),
            ["inspectNextButton"] = () => inspectUI?.InspectNext(),

            ["doneButton"] = GoHome,

            ["hide"] = ToggleUIVisibility,
            ["screenshot"] = TakeScreenshot,
        };
    }

    // -------------------------
    // Navigation
    // -------------------------
    private void GoHome()
    {
        rigBuilder.ApplyBaseRig(FIRST_DYNAMIC_CAM);

        if (cameraController != null)
            cameraController.goToPosition(CAM_START);

        videoUI?.Pause();
        videoUI?.Leave();

        sceneMode.SetVideoFov(false);
        sceneMode.SetModelDecalVisible(false);

        if (isMobileLayout && cameraController != null)
        {
            cameraController.SetTarget(rotationTarget, true);
            cameraController.goToPosition(CAM_MODEL_VIEW);
        }

        displayFlow.ShowHome();
        sceneMode.RefreshRotationState();
    }

    private void OpenVideo()
    {
        rigBuilder.ApplyBaseRig(FIRST_DYNAMIC_CAM);
        sceneMode.SetVideoFov(true);

        if (cameraController != null)
            cameraController.goToPosition(CAM_VIDEO);

        sceneMode.SetModelDecalVisible(false);
        displayFlow.ShowVideo();

        videoUI?.Enter();
        sceneMode.RefreshRotationState();
    }

    private void OpenModelRoot()
    {
        videoUI?.Pause();
        videoUI?.Leave();
        sceneMode.SetVideoFov(false);

        rigBuilder.ApplyModelRig(currentSimModel, FIRST_DYNAMIC_CAM);

        if (cameraController != null)
        {
            cameraController.goToPosition(CAM_MODEL_VIEW);
            cameraController.SetTarget(rotationTarget, true);
        }

        sceneMode.SetModelDecalVisible(true);
        OpenModelSelection();
    }

    private void OpenModelSelection()
    {
        displayFlow.ShowModelSelection();

        if (!isMobileLayout)
        {
            modelSelectionUI?.BuildIfNeeded(modelManager.LoadedModels, SelectSimModel);
            modelSelectionUI?.UpdateSelected(currentModelId, currentSimModel);
        }

        sceneMode.RefreshRotationState();
    }

    private void OpenSpecs()
    {
        if (isMobileLayout)
        {
            if (currentSimModel != null)
                specsUI.PopulateSpecs(ui.SpecsListContainer, currentSimModel);

            UpdateBrochureButtonState();
            return;
        }

        displayFlow.ShowSpecs();
        UpdateBrochureButtonState();

        if (currentSimModel != null)
            specsUI.PopulateSpecs(ui.SpecsListContainer, currentSimModel);

        modelSelectionUI?.UpdateSelected(currentModelId, currentSimModel);
        sceneMode.RefreshRotationState();
    }

    private void OpenInspect()
    {
        if (isMobileLayout)
        {
            inspectUI?.Rebuild(currentSimModel);
            return;
        }

        displayFlow.ShowInspect();
        inspectUI?.Rebuild(currentSimModel);
        modelSelectionUI?.UpdateSelected(currentModelId, currentSimModel);

        sceneMode.RefreshRotationState();
    }

    private void OpenModelPage(int pageIndex)
    {
        nav?.CloseOverlay();
        sceneMode.SetModelDecalVisible(true);

        switch (pageIndex)
        {
            case 0: OpenModelSelection(); break;
            case 1: OpenSpecs(); break;
            case 2: OpenInspect(); break;
        }
    }

    private void ToggleInfoOverlay()
    {
        nav?.ToggleOverlay();
        sceneMode.RefreshRotationState();
    }

    // -------------------------
    // Model selection
    // -------------------------
    private void SelectSimModel(string id)
    {
        if (currentModelId == id && currentSimModel != null)
        {
            modelSelectionUI?.UpdateSelected(currentModelId, currentSimModel);
            return;
        }

        currentModelId = id;

        if (!modelManager.Select(id))
        {
            currentSimModel = null;
            modelSelectionUI?.UpdateSelected(currentModelId, currentSimModel);
            UpdateBrochureButtonState();
            return;
        }

        currentSimModel = modelManager.CurrentModel;

        rigBuilder.ApplyModelRig(currentSimModel, FIRST_DYNAMIC_CAM);

        if (IsAnyModelScreenVisible() && userRotated)
        {
            if (cameraController != null)
                cameraController.goToPosition(CAM_MODEL_VIEW);

            inspectUI?.ResetView();
            userRotated = false;
        }

        modelSelectionUI?.UpdateSelected(currentModelId, currentSimModel);

        if (ScreenNavigator.IsVisible(ui.ModelSpecsScreen))
            specsUI.PopulateSpecs(ui.SpecsListContainer, currentSimModel);

        if (ScreenNavigator.IsVisible(ui.InspectModelScreen))
            inspectUI?.Rebuild(currentSimModel);

        UpdateBrochureButtonState();

        if (isMobileLayout && currentSimModel != null)
        {
            specsUI.PopulateSpecs(ui.SpecsListContainer, currentSimModel);
            inspectUI?.Rebuild(currentSimModel);
        }
    }

    // -------------------------
    // Misc helpers
    // -------------------------
    private bool IsAnyModelScreenVisible()
    {
        if (isMobileLayout)
            return ScreenNavigator.IsVisible(ui.HomeScreen);

        return ScreenNavigator.IsVisible(ui.ModelSelectionScreen) ||
               ScreenNavigator.IsVisible(ui.ModelSpecsScreen) ||
               ScreenNavigator.IsVisible(ui.InspectModelScreen);
    }

    private void ToggleUIVisibility()
    {
        var scope = nav != null ? nav.CurrentScreen : null;
        if (scope == null) scope = root;

        var right = scope?.Q<VisualElement>("RightContainer");
        if (right == null) return;

        uiHidden = !uiHidden;
        right.style.display = uiHidden ? DisplayStyle.None : DisplayStyle.Flex;

        if (isMobileLayout && ui.BottomPanel != null)
            ui.BottomPanel.style.display = uiHidden ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void TakeScreenshot()
    {
        if (ScreenshotHandler.Instance != null)
            ScreenshotHandler.Instance.CaptureScreenshot();
    }

    private void UpdateBrochureButtonState()
    {
        if (ui.DownloadPdfButton == null) return;

        bool hasPdf = currentSimModel != null &&
                      !string.IsNullOrWhiteSpace(currentSimModel.brochurePdfFile);

        ui.DownloadPdfButton.SetEnabled(hasPdf);
    }

    private void DownloadPdf()
    {
        if (currentSimModel == null) return;

        WebGLDownloadManager.Instance?.DownloadPdfFromServer(
            currentSimModel.brochurePdfFile,
            currentSimModel.brochureDownloadName
        );
    }

    public void NotifyUserRotated() => userRotated = true;

    private bool ModelExists(string id)
    {
        if (string.IsNullOrEmpty(id) || modelManager == null) return false;
        var list = modelManager.LoadedModels;
        for (int i = 0; i < list.Count; i++)
            if (list[i] != null && list[i].id == id)
                return true;
        return false;
    }

    private string GetFirstModelIdOrDefault(string fallback)
    {
        var list = modelManager?.LoadedModels;
        if (list != null && list.Count > 0 && list[0] != null && !string.IsNullOrEmpty(list[0].id))
            return list[0].id;
        return fallback;
    }
}