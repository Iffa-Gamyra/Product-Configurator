using System;
using System.Collections.Generic;
using System.Linq;
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

    // Camera index map (CorneaCameraDirector.LerpCameraPositions)
    private const int CAM_SWOOP = 0;
    private const int CAM_START = 1;
    private const int CAM_MODEL_VIEW = 2;
    private const int CAM_VIDEO = 3;
    private const int FIRST_DYNAMIC_CAM = 4;

    // Runtime UI (Desktop master root)
    private VisualElement root;

    // Screen roots (match your Desktop instance names)
    private VisualElement welcomeScreen;
    private VisualElement homeScreen;
    private VisualElement videoScreen;
    private VisualElement modelSelectionScreen;
    private VisualElement modelSpecsScreen;
    private VisualElement inspectModelScreen;
    private VisualElement infoOverlay;

    //info Overlay 
    private Button closeButton;

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

    private bool uiHidden = false;

    // Model data
    private List<SimulatorModel> loadedModels = new();
    private Dictionary<string, SimulatorModel> modelById = new();
    private SimulatorModel currentSimModel;
    public string selectedSimModel = "B1";
    private GameObject currentModelInstance;

    // Buttons
    private readonly Dictionary<string, Button> modelButtons = new();
    private readonly List<Button> inspectButtons = new();
    private int activeInspectIndex = -1;

    // UI actions
    private Dictionary<string, Action> actions;

    private bool uiInitialized = false;

    private enum UIMode { Home, Video, Model }

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        LoadModelsFromResources();
        BuildActions();
        BuildCameraRigBaseOnly();
    }

    private void Start()
    {
        if (videoPlayer != null)
            videoPlayer.prepareCompleted += OnVideoPrepared;

        SelectSimModel(string.IsNullOrEmpty(selectedSimModel) ? defaultModelId : selectedSimModel);
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

    }

    private void Update()
    {
        // Only update progress bar when video screen is visible 
        if (IsVisible(videoScreen) && progressBar != null && videoPlayer != null && videoPlayer.length > 0.0001)
            progressBar.value = (float)(videoPlayer.time / videoPlayer.length) * 100f;

        bool overlayOpen = IsOverlayOpen();
        bool allowRotation = IsAnyModelScreenVisible() && !overlayOpen;

        if (cameraController != null)
            cameraController.canRotate = allowRotation;

        // Disable rotation target completely when overlay is open
        if (rotationTarget != null)
            rotationTarget.gameObject.SetActive(allowRotation);
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
    }

    private void ShowOnly(VisualElement screen)
    {
        SetVisible(welcomeScreen, false);
        SetVisible(homeScreen, false);
        SetVisible(videoScreen, false);
        SetVisible(modelSelectionScreen, false);
        SetVisible(modelSpecsScreen, false);
        SetVisible(inspectModelScreen, false);

        SetVisible(screen, true);

        // If we leave inspect screen, clear inspect state & hide reset button
        if (screen != inspectModelScreen)
        {
            activeInspectIndex = -1;
            UpdateInspectSelectionUI();
        }
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

    // -------------------------
    // Loading models
    // -------------------------
    private void LoadModelsFromResources()
    {
        loadedModels = Resources.LoadAll<SimulatorModel>(resourcesPath).ToList();

        modelById.Clear();
        foreach (var m in loadedModels)
        {
            if (m == null) continue;
            if (string.IsNullOrWhiteSpace(m.id)) continue;
            if (modelById.ContainsKey(m.id)) continue;
            modelById[m.id] = m;
        }

        loadedModels = loadedModels
            .Where(m => m != null && !string.IsNullOrWhiteSpace(m.id))
            .OrderBy(m => m.id)
            .ToList();

        if (string.IsNullOrEmpty(selectedSimModel))
            selectedSimModel = defaultModelId;

        if (!modelById.ContainsKey(selectedSimModel) && loadedModels.Count > 0)
            selectedSimModel = loadedModels[0].id;
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
        if (cameraPosition == null || cameraPosition.Cornea == null) return;

        int inspectCount = (model != null && model.inspectPoints != null) ? model.inspectPoints.Length : 0;
        var rig = new Transform[FIRST_DYNAMIC_CAM + inspectCount];

        rig[CAM_SWOOP] = swoopPosition;
        rig[CAM_START] = startPosition;
        rig[CAM_MODEL_VIEW] = modelViewPosition;
        rig[CAM_VIDEO] = videoPosition;

        for (int i = 0; i < inspectCount; i++)
        {
            var p = model.inspectPoints[i];
            rig[FIRST_DYNAMIC_CAM + i] = (p != null) ? p.cameraAnchor : null;
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

        // bind all info buttons (one per screen is fine)
        foreach (var b in root.Query<Button>("infoButton").ToList())
        {
            b.clicked -= ToggleInfoOverlay;
            b.clicked += ToggleInfoOverlay;
        }

        // bind close inside overlay
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
        BuildCameraRigBaseOnly();
        ChangeVideoFOV(true);
        cameraPosition.goToPosition(CAM_VIDEO);

        SetRotationTargetActive(false);
        SetDecalForMode(UIMode.Video);

        ShowOnly(videoScreen);

        if (playButton != null && videoPlayer != null && !videoPlayer.isPrepared)
            playButton.SetEnabled(false);
    }

    private void OpenModelRoot()
    {
        PauseVideo();
        ChangeVideoFOV(false);

        BuildCameraRigForModel(currentSimModel);
        cameraPosition.goToPosition(CAM_MODEL_VIEW);

        SetRotationTargetActive(true);
        if (cameraController != null)
            cameraController.SetTarget(rotationTarget, true);

        SetDecalForMode(UIMode.Model);

        ShowOnly(modelSelectionScreen);
        BuildModelButtonsIfNeeded();
        UpdateModelSelectionUI();
    }

    private void OpenModelPage(int pageIndex)
    {
        SetDecalForMode(UIMode.Model);

        // Optional: if overlay is open, close it when navigating
        if (infoOverlay != null && infoOverlay.style.display != DisplayStyle.None)
            infoOverlay.style.display = DisplayStyle.None;

        if (pageIndex == 0)
        {
            ShowOnly(modelSelectionScreen);
            BuildModelButtonsIfNeeded();
            UpdateModelSelectionUI();
            return;
        }

        if (pageIndex == 1)
        {
            ShowOnly(modelSpecsScreen);
            UpdateBrochureButtonState();
            if (currentSimModel != null) PopulateSpecsUI(currentSimModel);
            return;
        }

        if (pageIndex == 2)
        {
            ShowOnly(inspectModelScreen);

            // Reset inspect state on entry (recommended)
            activeInspectIndex = -1;

            if (currentSimModel != null) PopulateInspectUI(currentSimModel);
            else UpdateInspectSelectionUI();

            return;
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

    }

    // -------------------------
    // Model selection / spawning
    // -------------------------
    private void SelectSimModel(string id)
    {
        selectedSimModel = id;

        if (!modelById.TryGetValue(id, out currentSimModel) || currentSimModel == null)
        {
            Debug.LogWarning($"SelectSimModel: id '{id}' not found in Resources/{resourcesPath}");
            UpdateModelSelectionUI();
            return;
        }

        if (currentSimModel.modelPrefab == null)
        {
            Debug.LogWarning($"SelectSimModel: model '{id}' has no prefab assigned.");
            UpdateModelSelectionUI();
            return;
        }

        if (currentModelInstance != null)
            Destroy(currentModelInstance);

        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = currentSimModel.modelPrefab.transform.rotation;
        currentModelInstance = Instantiate(currentSimModel.modelPrefab, pos, rot);

        BuildCameraRigForModel(currentSimModel);

        UpdateModelSelectionUI();

        // If user is on specs/inspect screens, keep content in sync
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
            kv.Value.EnableInClassList("active", kv.Key == selectedSimModel);

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
                        var chips = (spec.value ?? "")
                            .Split('|')
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrEmpty(s));

                        foreach (var chipText in chips)
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
            if (row == null) continue;

            var btn = row.Q<Button>("inspectButton");
            var label = row.Q<Label>("inspectLabel");

            if (label != null)
                label.text = point.label;

            if (btn != null)
            {
                inspectButtons.Add(btn);

                btn.clicked += () =>
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
                };
            }

            inspectListContainer.Add(row);
        }

        UpdateInspectSelectionUI();
    }

    private void UpdateInspectSelectionUI()
    {
        for (int i = 0; i < inspectButtons.Count; i++)
        {
            int cameraIndex = FIRST_DYNAMIC_CAM + i;
            var button = inspectButtons[i];
            if (button == null) continue;

            bool isActive = (cameraIndex == activeInspectIndex);

            // Keep button highlight if you want
            button.EnableInClassList("active", isActive);

            // Also highlight the label inside this row
            var row = button.parent; // button is inside the row
            var label = row?.Q<Label>("inspectLabel");
            if (label != null)
                label.EnableInClassList("active", isActive);
        }

        // Show Reset View ONLY after an inspect point is selected
        if (resetViewButton != null)
        {
            resetViewButton.style.display =
                (activeInspectIndex >= FIRST_DYNAMIC_CAM) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        int count = GetInspectCount();

        int first = FIRST_DYNAMIC_CAM;
        int last = FIRST_DYNAMIC_CAM + count - 1;

        bool inspecting = count > 0 && activeInspectIndex >= first && activeInspectIndex <= last;

        if (inspectPrevButton != null)
        {
            // Show only if we are inspecting and there is a previous inspect point
            bool showPrev = inspecting && activeInspectIndex > first;
            inspectPrevButton.style.display = showPrev ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (inspectNextButton != null)
        {
            // Show only if we are inspecting and there is a next inspect point
            bool showNext = inspecting && activeInspectIndex < last;
            inspectNextButton.style.display = showNext ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    // -------------------------
    // Video helpers
    // -------------------------
    private void PlayVideo()
    {
        if (videoPlayer == null) return;
        videoPlayer.Play();

        if (playButton != null) playButton.style.display = DisplayStyle.None;
        if (pauseButton != null) pauseButton.style.display = DisplayStyle.Flex;
    }

    private void PauseVideo()
    {
        if (videoPlayer == null) return;
        videoPlayer.Pause();

        if (playButton != null) playButton.style.display = DisplayStyle.Flex;
        if (pauseButton != null) pauseButton.style.display = DisplayStyle.None;
    }

    private void MuteVideo()
    {
        if (videoPlayer == null) return;
        videoPlayer.SetDirectAudioMute(0, true);

        if (muteButton != null) muteButton.style.display = DisplayStyle.None;
        if (unmuteButton != null) unmuteButton.style.display = DisplayStyle.Flex;
    }

    private void UnmuteVideo()
    {
        if (videoPlayer == null) return;
        videoPlayer.SetDirectAudioMute(0, false);

        if (muteButton != null) muteButton.style.display = DisplayStyle.Flex;
        if (unmuteButton != null) unmuteButton.style.display = DisplayStyle.None;
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

        if (playButton != null) playButton.style.display = DisplayStyle.None;
        if (pauseButton != null) pauseButton.style.display = DisplayStyle.Flex;
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        if (playButton != null)
            playButton.SetEnabled(true);
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

    private int GetInspectCount()
    => (currentSimModel != null && currentSimModel.inspectPoints != null)
        ? currentSimModel.inspectPoints.Length
        : 0;

    private void GoToInspectIndex(int cameraIndex)
    {
        activeInspectIndex = cameraIndex;
        cameraPosition.goToPosition(cameraIndex);
        UpdateInspectSelectionUI();
    }

    private void InspectNext()
    {
        int count = GetInspectCount();
        if (count <= 0) return;

        int first = FIRST_DYNAMIC_CAM;
        int last = FIRST_DYNAMIC_CAM + count - 1;

        // If not inspecting yet, start at first
        int next = (activeInspectIndex < first || activeInspectIndex > last)
            ? first
            : Mathf.Min(activeInspectIndex + 1, last);

        GoToInspectIndex(next);
    }

    private void InspectPrev()
    {
        int count = GetInspectCount();
        if (count <= 0) return;

        int first = FIRST_DYNAMIC_CAM;
        int last = FIRST_DYNAMIC_CAM + count - 1;

        // If not inspecting yet, start at first (or last—your choice). We'll start at first.
        int prev = (activeInspectIndex < first || activeInspectIndex > last)
            ? first
            : Mathf.Max(activeInspectIndex - 1, first);

        GoToInspectIndex(prev);
    }

    // -------------------------
    // Misc helpers
    // -------------------------
    private void ChangeVideoFOV(bool isVideo)
    {
        if (Camera.main == null) return;
        Camera.main.fieldOfView = isVideo ? video_FOV : normal_FOV;
    }

    private void SetRotationTargetActive(bool on)
    {
        if (rotationTarget != null)
            rotationTarget.gameObject.SetActive(on);
    }

    private void TakeScreenshot()
    {
        if (ScreenshotHandler.Instance != null)
            ScreenshotHandler.Instance.CaptureScreenshot();
    }

    private void ToggleUIVisibility()
    {
        // If RightContainer exists inside multiple screens, prefer the visible one
        VisualElement right = null;

        if (IsVisible(homeScreen)) right = homeScreen?.Q<VisualElement>("RightContainer");
        else if (IsVisible(videoScreen)) right = videoScreen?.Q<VisualElement>("RightContainer");
        else if (IsVisible(modelSelectionScreen)) right = modelSelectionScreen?.Q<VisualElement>("RightContainer");
        else if (IsVisible(modelSpecsScreen)) right = modelSpecsScreen?.Q<VisualElement>("RightContainer");
        else if (IsVisible(inspectModelScreen)) right = inspectModelScreen?.Q<VisualElement>("RightContainer");
        else right = root?.Q<VisualElement>("RightContainer");

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
}