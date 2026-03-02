using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class HomeScreen : MonoBehaviour
{
    [Header("Camera / World")]
    public CameraController cameraController;
    public EnableLocation cameraPosition;
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

    [Header("UI Model Selection Templates")]
    public VisualTreeAsset modelButtonTemplate;

    [Header("Spec Row Templates")]
    public VisualTreeAsset specRowTextTemplate;
    public VisualTreeAsset specRowBarTemplate;
    public VisualTreeAsset specRowToggleTemplate;
    public VisualTreeAsset specRowChipsTemplate;

    [Header("Inspect Row Template")]
    public VisualTreeAsset inspectRowTemplate;

    [Header("Video")]
    public float video_FOV;
    public float normal_FOV;

    // Camera 
    private const int CAM_SWOOP = 0;
    private const int CAM_START = 1;
    private const int CAM_MODEL_VIEW = 2;
    private const int CAM_VIDEO = 3;
    private const int FIRST_DYNAMIC_CAM = 4;
    private readonly List<int> inspectCameraIndices = new();
    private readonly Dictionary<string, Transform[]> cameraRigCache = new();
    private bool userRotated = false;

    // Runtime UI (Desktop master root)
    private VisualElement root;

    // Screen roots 
    private VisualElement welcomeScreen;
    private VisualElement homeScreen;
    private VisualElement videoScreen;
    private VisualElement modelSelectionScreen;
    private VisualElement modelSpecsScreen;
    private VisualElement inspectModelScreen;
    private VisualElement infoOverlay;

    // Video UI (scoped to VideoScreen)
    private VideoPlayer videoPlayer;
    private Button playButton, pauseButton, muteButton, unmuteButton, replayButton;
    private ProgressBar progressBar;

    // Model UI (scoped to specific screens)
    private VisualElement modelsContainer;        // ModelSelection
    private Label selectedModelInSpecScreen;                  // ModelSelection
    private Label selectedModelInInspectScreen;                  // ModelSelection
    private VisualElement specsListContainer;      // ModelSpecs
    private Button downloadPdfButton; // Specs screen only
    private VisualElement inspectListContainer;    // InspectModel
    private Button resetViewButton, inspectPrevButton, inspectNextButton; // InspectModel (show only when inspect point selected)

    //UI data
    private bool uiHidden = false;
    private List<VisualElement> allScreens;
    private VisualElement currentScreen;
    private Dictionary<string, Action> actions;
    private bool uiInitialized = false;

    // Model data
    private List<SimulatorModel> loadedModels = new();
    private Dictionary<string, SimulatorModel> modelById = new();
    private SimulatorModel currentSimModel;
    [SerializeField] private string currentModelId = "B1";
    private GameObject activeModelInstance;
    private readonly Dictionary<string, GameObject> modelInstanceCache = new();

    // Buttons
    private readonly Dictionary<string, Button> modelButtons = new();
    private readonly List<Button> inspectButtons = new();
    private int activeInspectIndex = -1;

    //Misc data
    private Camera mainCam;
    private Coroutine videoProgressRoutine;
    private static readonly WaitForSeconds VideoProgressWait = new WaitForSeconds(0.1f);

    private enum UIMode { Home, Video, Model }

    private void Awake()
    {
        mainCam = Camera.main;
        videoPlayer = GetComponent<VideoPlayer>();

        LoadModelsFromResources();
        BuildActions();
        BuildCameraRigBaseOnly();
    }

    private void Start()
    {
        // Ensure models loaded
        if (loadedModels == null || loadedModels.Count == 0)
            LoadModelsFromResources();

        // Pick a valid id if current is invalid
        if (string.IsNullOrEmpty(currentModelId))
            currentModelId = defaultModelId;

        if (!modelById.ContainsKey(currentModelId) && loadedModels.Count > 0)
            currentModelId = loadedModels[0].id;

        SelectSimModel(currentModelId);
    }

    private void OnEnable()
    {
        if (uiDocument == null) return;

        root = uiDocument.rootVisualElement;
        if (root == null) return;

        CacheScreens();


        if (!uiInitialized)
        {
            CacheUI();     
            BindButtons();
            BindInfoOverlayButtons();
            BuildModelButtonsIfNeeded();

            // Welcome screen start hook (kept from your old flow)
            if (welcomeScreen != null && homeScreen != null)
            {
                WelcomeScreenManager wsManager = new WelcomeScreenManager(welcomeScreen);
                wsManager.Start = () =>
                {
                    cameraPosition.goToPosition(CAM_START);
                    ShowOnly(homeScreen);

                    if (videoPlayer != null && !videoPlayer.isPrepared)
                        videoPlayer.Prepare();
                };
            }

            uiInitialized = true;
        }

        if (videoPlayer != null)
            videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void OnDisable()
    {

        if (videoPlayer != null)
            videoPlayer.prepareCompleted -= OnVideoPrepared;
    }

    // -------------------------
    // Screens (Desktop master)
    // -------------------------
    private void CacheScreens()
    {
        welcomeScreen = root.Q<VisualElement>("WelcomeScreen");
        homeScreen = root.Q<VisualElement>("HomeScreen");
        videoScreen = root.Q<VisualElement>("VideoScreen");
        modelSelectionScreen = root.Q<VisualElement>("ModelSelection");
        modelSpecsScreen = root.Q<VisualElement>("ModelSpecs");
        inspectModelScreen = root.Q<VisualElement>("InspectModel");
        infoOverlay = root.Q<VisualElement>("InfoOverlay");

        if (allScreens == null) allScreens = new List<VisualElement>(6);
        else allScreens.Clear();

        allScreens.Clear();
        allScreens.Add(welcomeScreen);
        allScreens.Add(homeScreen);
        allScreens.Add(videoScreen);
        allScreens.Add(modelSelectionScreen);
        allScreens.Add(modelSpecsScreen);
        allScreens.Add(inspectModelScreen);

        if (infoOverlay != null)
            infoOverlay.style.display = DisplayStyle.None;
    }

    private void ShowOnly(VisualElement screen)
    {
        if (screen == null) return;

        // Always close overlay when switching screens
        if (infoOverlay != null)
            infoOverlay.style.display = DisplayStyle.None;

        if (screen != videoScreen) StopVideoProgress();

        foreach (var s in allScreens)
            SetVisible(s, false);

        SetVisible(screen, true);
        currentScreen = screen;

        if (screen != inspectModelScreen)
        {
            activeInspectIndex = -1;
            UpdateInspectSelectionUI();
        }

        RefreshRotationState();
    }


    private static void SetVisible(VisualElement ve, bool visible)
    {
        if (ve == null) return;
        ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private static bool IsVisible(VisualElement ve)
        => ve != null && ve.style.display != DisplayStyle.None;

    private bool IsAnyModelScreenVisible()
        => IsVisible(modelSelectionScreen) || IsVisible(modelSpecsScreen) || IsVisible(inspectModelScreen);

    private bool IsOverlayOpen()
    => infoOverlay != null && infoOverlay.style.display != DisplayStyle.None;

    private static void SetButtonVisible(Button b, bool visible)
    {
        if (b == null) return;
        b.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void SetPlayingUI(bool playing)
    {
        SetButtonVisible(playButton, !playing);
        SetButtonVisible(pauseButton, playing);
    }

    private void SetMutedUI(bool muted)
    {
        SetButtonVisible(muteButton, !muted);
        SetButtonVisible(unmuteButton, muted);
    }
    // -------------------------
    // Loading models
    // -------------------------
    private void LoadModelsFromResources()
    {
        loadedModels.Clear();
        modelById.Clear();

        var all = Resources.LoadAll<SimulatorModel>(resourcesPath);
        for (int i = 0; i < all.Length; i++)
        {
            var m = all[i];
            if (m == null) continue;
            if (string.IsNullOrWhiteSpace(m.id)) continue;
            if (modelById.ContainsKey(m.id)) continue;

            modelById[m.id] = m;
            loadedModels.Add(m);
        }

        loadedModels.Sort((a, b) => string.CompareOrdinal(a.id, b.id));

        if (string.IsNullOrEmpty(currentModelId))
            currentModelId = defaultModelId;

        if (!modelById.ContainsKey(currentModelId) && loadedModels.Count > 0)
            currentModelId = loadedModels[0].id;
    }

    // -------------------------
    // Camera rig
    // -------------------------
    private void BuildCameraRigBaseOnly()
    {
        if (cameraPosition == null || cameraPosition.Cornea == null) return;

        var rig = new Transform[FIRST_DYNAMIC_CAM];
        rig[CAM_SWOOP] = swoopPosition;
        rig[CAM_START] = startPosition;
        rig[CAM_MODEL_VIEW] = modelViewPosition;
        rig[CAM_VIDEO] = videoPosition;

        cameraPosition.Cornea.LerpCameraPositions = rig;
    }

    private void BuildCameraRigForModel(SimulatorModel model)
    {
        if (cameraPosition == null || cameraPosition.Cornea == null || model == null) return;

        if (!cameraRigCache.TryGetValue(model.id, out var rig) || rig == null)
        {
            int inspectCount = (model.inspectPoints != null) ? model.inspectPoints.Length : 0;
            rig = new Transform[FIRST_DYNAMIC_CAM + inspectCount];

            rig[CAM_SWOOP] = swoopPosition;
            rig[CAM_START] = startPosition;
            rig[CAM_MODEL_VIEW] = modelViewPosition;
            rig[CAM_VIDEO] = videoPosition;

            for (int i = 0; i < inspectCount; i++)
            {
                var p = model.inspectPoints[i];
                rig[FIRST_DYNAMIC_CAM + i] = (p != null) ? p.cameraAnchor : null;
            }

            cameraRigCache[model.id] = rig;
        }

        cameraPosition.Cornea.LerpCameraPositions = rig;
        activeInspectIndex = -1;
    }

    // -------------------------
    // Decal Controller
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
    // Navigation / Actions
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

            ["play-button"] = PlayVideo,
            ["pause-button"] = PauseVideo,
            ["mute-button"] = MuteVideo,
            ["unmute-button"] = UnmuteVideo,
            ["replay-button"] = ReplayVideo,

            ["modelTCButton"] = () => OpenModelPage(0),
            ["specsTCButton"] = () => OpenModelPage(1),
            ["inspectTCButton"] = () => OpenModelPage(2),

            ["specsButton"] = () => OpenModelPage(1),
            ["inspectButton"] = () => OpenModelPage(2),
            ["viewModelsButton"] = () => OpenModelPage(0),
            ["viewSpecsButton"] = () => OpenModelPage(1),
            ["downloadButton"] = DownloadPdf,

            ["resetViewButton"] = ResetView,
            ["inspectPrevButton"] = InspectPrev,
            ["inspectNextButton"] = InspectNext,

            ["doneButton"] = GoHome,

            ["hide"] = ToggleUIVisibility,
            ["screenshot"] = TakeScreenshot,
        };
    }

    // Bind within each screen to avoid name collisions across templates
    private void BindButtons()
    {
        if (actions == null) return;

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
        if (scope == null) return;

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
    private void GoHome()
    {
      
        BuildCameraRigBaseOnly();
        cameraPosition.goToPosition(CAM_START);

        PauseVideo();
        ChangeVideoFOV(false);
        SetRotationTargetActive(false);
        SetDecalForMode(UIMode.Home);

        ShowOnly(homeScreen);
        
    }

    private void OpenVideo()
    {
        if (videoPlayer != null && !videoPlayer.isPrepared)
            videoPlayer.Prepare();

        BuildCameraRigBaseOnly();
        ChangeVideoFOV(true);
        cameraPosition.goToPosition(CAM_VIDEO);

        SetRotationTargetActive(false);
        SetDecalForMode(UIMode.Video);

        ShowOnly(videoScreen);
        SetPlayingUI(videoPlayer != null && videoPlayer.isPlaying);
        SetMutedUI(videoPlayer != null && videoPlayer.GetDirectAudioMute(0));
        StartVideoProgress();

        if (playButton != null && videoPlayer != null && !videoPlayer.isPrepared)
            playButton.SetEnabled(false);

    }


    private void OpenModelRoot()
    {
        PauseVideo();
        ChangeVideoFOV(false);

        BuildCameraRigForModel(currentSimModel);
        cameraPosition.goToPosition(CAM_MODEL_VIEW);

        
        if (cameraController != null)
            cameraController.SetTarget(rotationTarget, true);

        SetDecalForMode(UIMode.Model);
        OpenModelSelection();
    }

    private void OpenModelSelection()
    {
        ShowOnly(modelSelectionScreen);
        BuildModelButtonsIfNeeded();
        UpdateModelSelectionUI();
    }

    private void OpenSpecs()
    {
        ShowOnly(modelSpecsScreen);
        UpdateBrochureButtonState();
        if (currentSimModel != null) PopulateSpecsUI(currentSimModel);
    }

    private void OpenInspect()
    {
        ShowOnly(inspectModelScreen);
        activeInspectIndex = -1;

        if (currentSimModel != null) PopulateInspectUI(currentSimModel);
        else UpdateInspectSelectionUI();
    }

    private void OpenModelPage(int pageIndex)
    {
        if (infoOverlay != null && infoOverlay.style.display != DisplayStyle.None)
            infoOverlay.style.display = DisplayStyle.None;

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
        if (infoOverlay == null) return;

        bool isOpen = infoOverlay.style.display == DisplayStyle.Flex;

        // Just toggle
        infoOverlay.style.display = isOpen
            ? DisplayStyle.None
            : DisplayStyle.Flex;

        // Always bring to front when opening
        if (!isOpen)
            infoOverlay.BringToFront();

        RefreshRotationState();
    }

    // -------------------------
    // Model selection / spawning
    // -------------------------
    private void SelectSimModel(string id)
    {
        if (currentModelId == id && activeModelInstance != null)
        {
            UpdateModelSelectionUI();
            return;
        }
        currentModelId = id;

        bool valid =
            modelById.TryGetValue(id, out var model) &&
            model != null &&
            model.modelPrefab != null;

        if (!valid)
        {
            currentSimModel = null;

            if (activeModelInstance != null)
                activeModelInstance.SetActive(false);

            UpdateModelSelectionUI();
            UpdateBrochureButtonState();
            return;
        }

        currentSimModel = model;

        if (activeModelInstance != null)
            activeModelInstance.SetActive(false);

        if (!modelInstanceCache.TryGetValue(currentSimModel.id, out var inst) || inst == null)
        {
            inst = Instantiate(currentSimModel.modelPrefab);

            if (spawnPoint != null)
            {
                inst.transform.SetParent(spawnPoint, worldPositionStays: false);
                inst.transform.SetLocalPositionAndRotation(Vector3.zero, currentSimModel.modelPrefab.transform.localRotation);
            }
            else
            {
                inst.transform.SetPositionAndRotation(Vector3.zero, currentSimModel.modelPrefab.transform.rotation);
            }

            modelInstanceCache[currentSimModel.id] = inst;
        }

        inst.SetActive(true);
        activeModelInstance = inst;

        BuildCameraRigForModel(currentSimModel);
        if (IsAnyModelScreenVisible() && userRotated)
        {
            cameraPosition.goToPosition(CAM_MODEL_VIEW);
            activeInspectIndex = -1;
            UpdateInspectSelectionUI();
            userRotated = false; 
        }
        UpdateModelSelectionUI();

        if (IsVisible(modelSpecsScreen)) PopulateSpecsUI(currentSimModel);
        if (IsVisible(inspectModelScreen)) PopulateInspectUI(currentSimModel);

        UpdateBrochureButtonState();
    }

    // -------------------------
    // UI Cache + Dynamic lists
    // -------------------------
    private void CacheUI()
    {
        // Video screen
        playButton = videoScreen?.Q<Button>("play-button");
        pauseButton = videoScreen?.Q<Button>("pause-button");
        muteButton = videoScreen?.Q<Button>("mute-button");
        unmuteButton = videoScreen?.Q<Button>("unmute-button");
        replayButton = videoScreen?.Q<Button>("replay-button");
        progressBar = videoScreen?.Q<ProgressBar>("progress-bar");


        // Model selection screen
        modelsContainer = modelSelectionScreen?.Q<VisualElement>("Models");

        // Specs screen
        specsListContainer = modelSpecsScreen?.Q<VisualElement>("SpecsContainer");
        downloadPdfButton = modelSpecsScreen?.Q<Button>("downloadButton");
        selectedModelInSpecScreen = modelSpecsScreen?.Q<Label>("selectedModel");

        // Inspect screen
        inspectListContainer = inspectModelScreen?.Q<VisualElement>("InspectContainer");
        selectedModelInInspectScreen = inspectModelScreen?.Q<Label>("selectedModel");
        resetViewButton = inspectModelScreen?.Q<Button>("resetViewButton");
        inspectPrevButton = inspectModelScreen?.Q<Button>("inspectPrevButton");
        inspectNextButton = inspectModelScreen?.Q<Button>("inspectNextButton");
        if (resetViewButton != null)
            resetViewButton.style.display = DisplayStyle.None;
    }

    private void BuildModelButtonsIfNeeded()
    {
        if (modelsContainer == null || modelButtonTemplate == null) return;

        // Only build once unless you intentionally want to rebuild every time
        if (modelsContainer.childCount > 0 && modelButtons.Count > 0) return;

        modelButtons.Clear();
        modelsContainer.Clear();

        foreach (var m in loadedModels)
        {
            if (m == null || string.IsNullOrWhiteSpace(m.id)) continue;

            var instance = modelButtonTemplate.Instantiate();
            var btn = instance.Q<Button>("modelButtonTemplate");
            if (btn == null)
            {
                Debug.LogError("Model button named 'modelButtonTemplate' not found inside modelButtonTemplate UXML.");
                continue;
            }

            btn.name = m.id;
            btn.text = m.modelName;

            // Capture id
            string mid = m.id;
            btn.clicked += () => SelectSimModel(mid);

            modelsContainer.Add(instance);
            modelButtons[m.id] = btn;
        }

        UpdateModelSelectionUI();
    }

    private void UpdateModelSelectionUI()
    {
        foreach (var kv in modelButtons)
            kv.Value.EnableInClassList("active", kv.Key == currentModelId);

        if (selectedModelInSpecScreen != null && currentSimModel != null)
            selectedModelInSpecScreen.text = currentSimModel.modelName;

        if (selectedModelInInspectScreen != null && currentSimModel != null)
            selectedModelInInspectScreen.text = currentSimModel.modelName;
    }

    private void PopulateSpecsUI(SimulatorModel model)
    {
        if (specsListContainer == null) return;

        specsListContainer.Clear();
        if (model.specs == null) return;

        foreach (var spec in model.specs)
        {
            VisualElement row = null;

            switch (spec.type)
            {
                case SimulatorModel.SpecType.Text:
                    row = specRowTextTemplate?.Instantiate();
                    if (row == null) break;
                    row.Q<Label>("specLabel").text = spec.label;
                    row.Q<Label>("specValue").text = spec.value;
                    break;

                case SimulatorModel.SpecType.Bar:
                    row = specRowBarTemplate?.Instantiate();
                    if (row == null) break;
                    row.Q<Label>("specLabel").text = spec.label;
                    var valueLabel = row.Q<Label>("specValue");
                    if (valueLabel != null) valueLabel.text = spec.value;

                    var bar = row.Q<ProgressBar>("specBar");
                    if (bar != null)
                    {
                        bar.lowValue = 0f;
                        bar.highValue = Mathf.Max(0.0001f, spec.max);
                        bar.value = Mathf.Clamp(spec.current, 0f, bar.highValue);
                    }
                    break;

                case SimulatorModel.SpecType.InvertedBar:
                    row = specRowBarTemplate?.Instantiate();
                    if (row == null) break;
                    row.Q<Label>("specLabel").text = spec.label;

                    var valueLabelInv = row.Q<Label>("specValue");
                    if (valueLabelInv != null) valueLabelInv.text = spec.value;

                    var invBar = row.Q<ProgressBar>("specBar");
                    if (invBar != null)
                    {
                        invBar.lowValue = 0f;
                        invBar.highValue = Mathf.Max(0.0001f, spec.max);

                        float clamped = Mathf.Clamp(spec.current, 0f, invBar.highValue);
                        invBar.value = invBar.highValue - clamped;
                    }
                    break;

                case SimulatorModel.SpecType.Toggle:
                    row = specRowToggleTemplate?.Instantiate();
                    if (row == null) break;
                    row.Q<Label>("specLabel").text = spec.label;

                    var toggle = row.Q<Toggle>("specToggle");
                    if (toggle != null)
                    {
                        toggle.SetEnabled(false);
                        toggle.value = (spec.toggle == SimulatorModel.ToggleState.Yes ||
                                        spec.toggle == SimulatorModel.ToggleState.Optional);
                        toggle.text = spec.toggle.ToString();
                    }
                    break;

                case SimulatorModel.SpecType.Chips:
                    row = specRowChipsTemplate?.Instantiate();
                    if (row == null) break;
                    row.Q<Label>("specLabel").text = spec.label;

                    var chipsContainer = row.Q<VisualElement>("chipContainer");
                    if (chipsContainer != null)
                    {
                        chipsContainer.Clear();
                        foreach (var chipText in SplitChipsNoLinq(spec.value))
                        {
                            var chip = new Label(chipText);
                            chip.AddToClassList("chip");
                            chipsContainer.Add(chip);
                        }
                    }
                    break;
            }

            if (row != null)
                specsListContainer.Add(row);
        }
    }

    private void PopulateInspectUI(SimulatorModel model)
    {
        if (inspectListContainer == null || inspectRowTemplate == null) return;

        inspectListContainer.Clear();
        inspectButtons.Clear();
        inspectCameraIndices.Clear();
        activeInspectIndex = -1;

        if (model.inspectPoints == null)
        {
            UpdateInspectSelectionUI();
            return;
        }

        for (int i = 0; i < model.inspectPoints.Length; i++)
        {
            int cameraIndex = FIRST_DYNAMIC_CAM + i;  
            var point = model.inspectPoints[i];
            if (point == null) continue;

            var row = inspectRowTemplate.Instantiate();
            var btn = row.Q<Button>("inspectButton");
            var label = row.Q<Label>("inspectLabel");

            if (label != null)
                label.text = point.label;

            if (btn != null)
            {
                inspectButtons.Add(btn);
                inspectCameraIndices.Add(cameraIndex); 

                btn.clicked += () => OnInspectClicked(cameraIndex);
            }

            inspectListContainer.Add(row);
        }

        UpdateInspectSelectionUI();
    }

    private void OnInspectClicked(int cameraIndex)
    {
        if (activeInspectIndex == cameraIndex)
        {
            activeInspectIndex = -1;
            cameraPosition.goToPosition(CAM_MODEL_VIEW);
        }
        else
        {
            activeInspectIndex = cameraIndex;
            cameraPosition.goToPosition(cameraIndex);
        }

        UpdateInspectSelectionUI();
    }
    private void UpdateInspectSelectionUI()
    {
        // Highlight the selected inspect row/button
        for (int i = 0; i < inspectButtons.Count; i++)
        {
            int cameraIndex = inspectCameraIndices[i];
            var button = inspectButtons[i];
            if (button == null) continue;

            bool isActive = (cameraIndex == activeInspectIndex);
            button.EnableInClassList("active", isActive);

            
            var row = button.parent;
            var label = row?.Q<Label>("inspectLabel");
            if (label != null)
                label.EnableInClassList("active", isActive);
        }

        // Reset button visible only when inspecting an inspect point
        if (resetViewButton != null)
        {
            resetViewButton.style.display =
                (GetActiveInspectListIndex() >= 0) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        
        int idx = GetActiveInspectListIndex();
        bool inspecting = idx >= 0;

        bool canPrev = inspecting && idx > 0;
        bool canNext = inspecting && idx < inspectCameraIndices.Count - 1;

        if (inspectPrevButton != null)
        {
            var container = inspectPrevButton.parent;
            if (container != null)
                container.style.display = canPrev ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (inspectNextButton != null)
        {
            var container = inspectNextButton.parent;
            if (container != null)
                container.style.display = canNext ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    // -------------------------
    // Video helpers
    // -------------------------
    private void PlayVideo()
    {
        if (videoPlayer == null) return;
        videoPlayer.Play();
        SetPlayingUI(true);
    }

    private void PauseVideo()
    {
        if (videoPlayer == null) return;
        videoPlayer.Pause();
        SetPlayingUI(false);
    }

    private void MuteVideo()
    {
        if (videoPlayer == null) return;
        videoPlayer.SetDirectAudioMute(0, true);
        SetMutedUI(true);
    }

    private void UnmuteVideo()
    {
        if (videoPlayer == null) return;
        videoPlayer.SetDirectAudioMute(0, false);
        SetMutedUI(false);
    }

    private void ReplayVideo()
    {
        if (videoPlayer == null) return;

        if (!videoPlayer.isPrepared)
        {
            videoPlayer.Prepare();
            return;
        }

        videoPlayer.time = 0;
        if (videoPlayer.frameCount > 0)
            videoPlayer.frame = 0;

        videoPlayer.Play();
        SetPlayingUI(true);
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        if (playButton != null)
            playButton.SetEnabled(true);
    }

    private void StartVideoProgress()
    {
        if (!IsVisible(videoScreen)) return;
        if (videoProgressRoutine != null) StopCoroutine(videoProgressRoutine);
        videoProgressRoutine = StartCoroutine(VideoProgressLoop());
    }

    private void StopVideoProgress()
    {
        if (videoProgressRoutine != null) StopCoroutine(videoProgressRoutine);
        videoProgressRoutine = null;
    }

    private IEnumerator VideoProgressLoop()
    {
       
        while (IsVisible(videoScreen))
        {
            if (progressBar != null && videoPlayer != null && videoPlayer.isPrepared && videoPlayer.length > 0.0001)
                progressBar.value = (float)(videoPlayer.time / videoPlayer.length) * 100f;

            yield return VideoProgressWait;
        }
    }

    // -------------------------
    // Reset View
    // -------------------------
    private void ResetView()
    {
        if (!IsAnyModelScreenVisible()) return;

        activeInspectIndex = -1;
        cameraPosition.goToPosition(CAM_MODEL_VIEW);
        UpdateInspectSelectionUI();
    }


    // -------------------------
    // Inspect View
    // -------------------------

    private void GoToInspectIndex(int cameraIndex)
    {
        activeInspectIndex = cameraIndex;
        cameraPosition.goToPosition(cameraIndex);
        UpdateInspectSelectionUI();
    }

    private int GetActiveInspectListIndex()
    {
        for (int i = 0; i < inspectCameraIndices.Count; i++)
            if (inspectCameraIndices[i] == activeInspectIndex)
                return i;

        return -1;
    }
    private void InspectNext()
    {
        int count = inspectCameraIndices.Count;
        if (count == 0) return;

        int idx = GetActiveInspectListIndex();

        // If not currently inspecting, jump to first inspect point
        int nextIdx = (idx < 0) ? 0 : Mathf.Min(idx + 1, count - 1);

        GoToInspectIndex(inspectCameraIndices[nextIdx]);
    }

    private void InspectPrev()
    {
        int count = inspectCameraIndices.Count;
        if (count == 0) return;

        int idx = GetActiveInspectListIndex();

        // If not currently inspecting, jump to first inspect point (your chosen behavior)
        int prevIdx = (idx < 0) ? 0 : Mathf.Max(idx - 1, 0);

        GoToInspectIndex(inspectCameraIndices[prevIdx]);
    }

    // -------------------------
    // Misc helpers
    // -------------------------
    private void ChangeVideoFOV(bool isVideo)
    {
        if (mainCam == null) return;
        mainCam.fieldOfView = isVideo ? video_FOV : normal_FOV;
    }

    private void RefreshRotationState()
    {
        bool allowRotation = IsAnyModelScreenVisible() && !IsOverlayOpen();
        if (cameraController != null)
            cameraController.canRotate = allowRotation;

        SetRotationTargetActive(allowRotation);
    }

    private void SetRotationTargetActive(bool on)
    {
        if (rotationTarget != null)
            rotationTarget.gameObject.SetActive(on);
    }

    private static IEnumerable<string> SplitChipsNoLinq(string s)
    {
        if (string.IsNullOrEmpty(s))
            yield break;

        int start = 0;
        for (int i = 0; i <= s.Length; i++)
        {
            if (i == s.Length || s[i] == '|')
            {
                int len = i - start;
                if (len > 0)
                {
                    string part = s.Substring(start, len).Trim();
                    if (part.Length > 0)
                        yield return part;
                }
                start = i + 1;
            }
        }
    }

    private void TakeScreenshot()
    {
        if (ScreenshotHandler.Instance != null)
            ScreenshotHandler.Instance.CaptureScreenshot();
    }

    private void ToggleUIVisibility()
    {
        VisualElement scope = currentScreen ?? root;
        var right = scope?.Q<VisualElement>("RightContainer");
        if (right == null) return;

        uiHidden = !uiHidden;
        right.style.display = uiHidden ? DisplayStyle.None : DisplayStyle.Flex;
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
}