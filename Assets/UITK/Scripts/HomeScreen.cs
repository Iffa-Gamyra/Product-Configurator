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

    [Header("UI Document (Desktop Master)")]
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
    private VisualElement welcomeScreen, homeScreen, videoScreen, modelSelectionScreen, modelSpecsScreen, inspectModelScreen, infoOverlay;
    private List<VisualElement> allScreens;

    // Cached controls
    private VisualElement modelsContainer;
    private Label selectedModelInSpecScreen;
    private Label selectedModelInInspectScreen;
    private VisualElement specsListContainer;
    private Button downloadPdfButton;

    private VisualElement inspectListContainer;
    private Button resetViewButton, inspectPrevButton, inspectNextButton;

    // Video UI controls
    private Button playButton, pauseButton, muteButton, unmuteButton, replayButton;
    private ProgressBar progressBar;

    // State
    private bool uiHidden = false;
    private bool uiInitialized = false;

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

    // Video + camera
    private VideoPlayer videoPlayer;
    private Camera mainCam;

    private enum UIMode { Home, Video, Model }
    private Dictionary<string, Action> actions;

    private void Awake()
    {
        mainCam = Camera.main;
        videoPlayer = GetComponent<VideoPlayer>();

        modelManager = new ModelManager(resourcesPath, spawnPoint);

        // camera rig builder
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

        CacheScreensAndModules();
        CacheUI();

        // Video module
        if (videoUI == null)
        {
            videoUI = new VideoUIController(
                this,
                videoPlayer,
                playButton,
                pauseButton,
                muteButton,
                unmuteButton,
                replayButton,
                progressBar,
                () => ScreenNavigator.IsVisible(videoScreen)
            );
        }

        // Specs module 
        if (specsUI == null)
        {
            specsUI = new SpecsUIController(
                specRowTextTemplate,
                specRowBarTemplate,
                specRowToggleTemplate,
                specRowChipsTemplate
            );
        }

        // Model selection UI module 
        modelSelectionUI = new ModelSelectionUIController(
            modelsContainer,
            modelButtonTemplate,
            selectedModelInSpecScreen,
            selectedModelInInspectScreen
        );

        // Inspect module 
        inspectUI = new InspectUIController(
            cameraController,
            CAM_MODEL_VIEW,
            FIRST_DYNAMIC_CAM,
            inspectListContainer,
            inspectRowTemplate,
            resetViewButton,
            inspectPrevButton,
            inspectNextButton
        );

        if (!uiInitialized)
        {
            BindButtons();
            BindInfoOverlayButtons();

            // Build model buttons once UI exists
            modelSelectionUI.BuildIfNeeded(modelManager.LoadedModels, SelectSimModel);
            modelSelectionUI.UpdateSelected(currentModelId, currentSimModel);

            if (welcomeScreen != null && homeScreen != null)
            {
                WelcomeScreenManager wsManager = new WelcomeScreenManager(welcomeScreen);
                wsManager.Start = () =>
                {
                    cameraController.goToPosition(CAM_START);
                    nav.ShowOnly(homeScreen);
                    RefreshRotationState();

                    if (videoPlayer != null && !videoPlayer.isPrepared)
                        videoPlayer.Prepare();
                };
            }

            uiInitialized = true;
        }
        else
        {
            // If re-enabled, ensure model buttons exist
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
    // Cache screens + nav
    // -------------------------
    private void CacheScreensAndModules()
    {
        welcomeScreen = root.Q<VisualElement>("WelcomeScreen");
        homeScreen = root.Q<VisualElement>("HomeScreen");
        videoScreen = root.Q<VisualElement>("VideoScreen");
        modelSelectionScreen = root.Q<VisualElement>("ModelSelection");
        modelSpecsScreen = root.Q<VisualElement>("ModelSpecs");
        inspectModelScreen = root.Q<VisualElement>("InspectModel");
        infoOverlay = root.Q<VisualElement>("InfoOverlay");

        if (allScreens == null) allScreens = new List<VisualElement>(6);
        allScreens.Clear();
        allScreens.Add(welcomeScreen);
        allScreens.Add(homeScreen);
        allScreens.Add(videoScreen);
        allScreens.Add(modelSelectionScreen);
        allScreens.Add(modelSpecsScreen);
        allScreens.Add(inspectModelScreen);

        if (infoOverlay != null)
            infoOverlay.style.display = DisplayStyle.None;

        nav = new ScreenNavigator(allScreens, infoOverlay);
    }

    private void CacheUI()
    {
        // Video controls
        playButton = videoScreen?.Q<Button>("play-button");
        pauseButton = videoScreen?.Q<Button>("pause-button");
        muteButton = videoScreen?.Q<Button>("mute-button");
        unmuteButton = videoScreen?.Q<Button>("unmute-button");
        replayButton = videoScreen?.Q<Button>("replay-button");
        progressBar = videoScreen?.Q<ProgressBar>("progress-bar");

        // Model selection
        modelsContainer = modelSelectionScreen?.Q<VisualElement>("Models");

        // Specs
        specsListContainer = modelSpecsScreen?.Q<VisualElement>("SpecsContainer");
        downloadPdfButton = modelSpecsScreen?.Q<Button>("downloadButton");
        selectedModelInSpecScreen = modelSpecsScreen?.Q<Label>("selectedModel");

        // Inspect
        inspectListContainer = inspectModelScreen?.Q<VisualElement>("InspectContainer");
        selectedModelInInspectScreen = inspectModelScreen?.Q<Label>("selectedModel");
        resetViewButton = inspectModelScreen?.Q<Button>("resetViewButton");
        inspectPrevButton = inspectModelScreen?.Q<Button>("inspectPrevButton");
        inspectNextButton = inspectModelScreen?.Q<Button>("inspectNextButton");

        if (resetViewButton != null)
            resetViewButton.style.display = DisplayStyle.None;
    }

    // -------------------------
    // Actions / binding
    // -------------------------
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

    private void BindButtons()
    {
        BindButtonsIn(welcomeScreen);
        BindButtonsIn(homeScreen);
        BindButtonsIn(videoScreen);
        BindButtonsIn(modelSelectionScreen);
        BindButtonsIn(modelSpecsScreen);
        BindButtonsIn(inspectModelScreen);
        BindButtonsIn(infoOverlay);
    }

    private void BindButtonsIn(VisualElement scope)
    {
        if (scope == null || actions == null) return;

        foreach (var kv in actions)
        {
            var btn = scope.Q<Button>(kv.Key);
            if (btn == null) continue;

            btn.clicked -= kv.Value;
            btn.clicked += kv.Value;
        }
    }

    private void BindInfoOverlayButtons()
    {
        if (root == null) return;

        root.Query<Button>("infoButton").ForEach(b =>
        {
            b.clicked -= ToggleInfoOverlay;
            b.clicked += ToggleInfoOverlay;
        });

        var close = infoOverlay?.Q<Button>("closeButton");
        if (close != null)
        {
            close.clicked -= ToggleInfoOverlay;
            close.clicked += ToggleInfoOverlay;
        }
    }

    // -------------------------
    // Navigation
    // -------------------------
    private void GoHome()
    {
        rigBuilder.ApplyBaseRig(FIRST_DYNAMIC_CAM);
        cameraController.goToPosition(CAM_START);

        videoUI?.Pause();
        videoUI?.Leave();

        ChangeVideoFOV(false);
        SetRotationTargetActive(false);
        SetDecalForMode(UIMode.Home);

        nav.ShowOnly(homeScreen);
        RefreshRotationState();
    }

    private void OpenVideo()
    {
        rigBuilder.ApplyBaseRig(FIRST_DYNAMIC_CAM);
        ChangeVideoFOV(true);
        cameraController.goToPosition(CAM_VIDEO);

        SetRotationTargetActive(false);
        SetDecalForMode(UIMode.Video);

        nav.ShowOnly(videoScreen);
        videoUI?.Enter();

        RefreshRotationState();
    }

    private void OpenModelRoot()
    {
        videoUI?.Pause();
        videoUI?.Leave();
        ChangeVideoFOV(false);

        rigBuilder.ApplyModelRig(currentSimModel, FIRST_DYNAMIC_CAM);
        cameraController.goToPosition(CAM_MODEL_VIEW);

        if (cameraController != null)
            cameraController.SetTarget(rotationTarget, true);

        SetDecalForMode(UIMode.Model);
        OpenModelSelection();
    }

    private void OpenModelSelection()
    {
        nav.ShowOnly(modelSelectionScreen);

        modelSelectionUI?.BuildIfNeeded(modelManager.LoadedModels, SelectSimModel);
        modelSelectionUI?.UpdateSelected(currentModelId, currentSimModel);

        RefreshRotationState();
    }

    private void OpenSpecs()
    {
        nav.ShowOnly(modelSpecsScreen);
        UpdateBrochureButtonState();

        if (currentSimModel != null)
            specsUI.PopulateSpecs(specsListContainer, currentSimModel);

        modelSelectionUI?.UpdateSelected(currentModelId, currentSimModel);
        RefreshRotationState();
    }

    private void OpenInspect()
    {
        nav.ShowOnly(inspectModelScreen);

        inspectUI?.Rebuild(currentSimModel);
        modelSelectionUI?.UpdateSelected(currentModelId, currentSimModel);

        RefreshRotationState();
    }

    private void OpenModelPage(int pageIndex)
    {
        nav.CloseOverlay();
        SetDecalForMode(UIMode.Model);

        switch (pageIndex)
        {
            case 0: OpenModelSelection(); break;
            case 1: OpenSpecs(); break;
            case 2: OpenInspect(); break;
        }
    }

    private void ToggleInfoOverlay()
    {
        nav.ToggleOverlay();
        RefreshRotationState();
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
            cameraController.goToPosition(CAM_MODEL_VIEW);
            inspectUI?.ResetView();
            userRotated = false;
        }

        modelSelectionUI?.UpdateSelected(currentModelId, currentSimModel);

        if (ScreenNavigator.IsVisible(modelSpecsScreen))
            specsUI.PopulateSpecs(specsListContainer, currentSimModel);

        if (ScreenNavigator.IsVisible(inspectModelScreen))
            inspectUI?.Rebuild(currentSimModel);

        UpdateBrochureButtonState();
    }

    // -------------------------
    // Decal
    // -------------------------
    private void SetDecalForMode(UIMode mode)
    {
        if (decalController == null) return;

        bool wantDecal = (mode == UIMode.Model);

        if (wantDecal && !decalController.IsOpaque)
            decalController.StartFadeInAndScaleUp();
        else if (!wantDecal && decalController.IsOpaque)
            decalController.StartFadeOutAndScaleDown();
    }

    // -------------------------
    // Misc helpers
    // -------------------------
    private void ChangeVideoFOV(bool isVideo)
    {
        if (mainCam == null) return;
        mainCam.fieldOfView = isVideo ? video_FOV : normal_FOV;
    }

    private bool IsAnyModelScreenVisible()
        => ScreenNavigator.IsVisible(modelSelectionScreen) ||
           ScreenNavigator.IsVisible(modelSpecsScreen) ||
           ScreenNavigator.IsVisible(inspectModelScreen);

    private void RefreshRotationState()
    {
        bool allowRotation = IsAnyModelScreenVisible() && (nav != null && !nav.IsOverlayOpen());
        if (cameraController != null)
            cameraController.canRotate = allowRotation;

        SetRotationTargetActive(allowRotation);
    }

    private void SetRotationTargetActive(bool on)
    {
        if (rotationTarget != null)
            rotationTarget.gameObject.SetActive(on);
    }

    private void ToggleUIVisibility()
    {
        var scope = nav != null ? nav.CurrentScreen : null;
        if (scope == null) scope = root;

        var right = scope?.Q<VisualElement>("RightContainer");
        if (right == null) return;

        uiHidden = !uiHidden;
        right.style.display = uiHidden ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void TakeScreenshot()
    {
        if (ScreenshotHandler.Instance != null)
            ScreenshotHandler.Instance.CaptureScreenshot();
    }

    private void UpdateBrochureButtonState()
    {
        if (downloadPdfButton == null) return;

        bool hasPdf = currentSimModel != null &&
                      !string.IsNullOrWhiteSpace(currentSimModel.brochurePdfFile);

        downloadPdfButton.SetEnabled(hasPdf);
    }

    private void DownloadPdf()
    {
        if (currentSimModel == null) return;

        WebGLDownloadManager.Instance?.DownloadPdfFromStreamingAssets(
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