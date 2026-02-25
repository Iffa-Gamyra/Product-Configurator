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

    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("UI Sections")]
    public VisualTreeAsset homeUxml;
    public VisualTreeAsset videoUxml;
    public VisualTreeAsset[] modelUxmlPages;  

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

    // Runtime UI
    private VisualElement root;
    private VisualTreeAsset currentTree;

    private VideoPlayer videoPlayer;
    private Button playButton, pauseButton, muteButton, unmuteButton;
    private ProgressBar progressBar;

    private VisualElement modelsContainer;
    private Label selectedModel;
    private VisualElement specsListContainer;
    private VisualElement inspectListContainer;

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


    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        LoadModelsFromResources();   
        BuildActions();
        BuildCameraRigBaseOnly();

    }
    void Start()
    {
        root = uiDocument.rootVisualElement;

        var welcomeScreen = root.Q("WelcomeScreen");
        var homeScreen = root.Q("HomeScreen");

        if(welcomeScreen != null && homeScreen != null)
        {
            WelcomeScreenManager wsManager = new WelcomeScreenManager(welcomeScreen);
            wsManager.Start = () =>
            {
                cameraPosition.goToPosition(CAM_START);
                welcomeScreen.Display(false);
                homeScreen.Display(true);

                if (videoPlayer != null && !videoPlayer.isPrepared) videoPlayer.Prepare();
            };
        }

        if (videoPlayer != null)
            videoPlayer.prepareCompleted += OnVideoPrepared;

        SelectSimModel(string.IsNullOrEmpty(selectedSimModel) ? defaultModelId : selectedSimModel);
    }

    private void OnEnable()
    {
        if (uiDocument == null) return;

        root = uiDocument.rootVisualElement;

        if (currentTree == null && uiDocument.visualTreeAsset != null)
            currentTree = uiDocument.visualTreeAsset;

        CacheUI();
        BindButtons();
        BuildModelButtonsIfNeeded();
        PopulateCurrentPage();
    }

    private void Update()
    {
        if (progressBar != null && videoPlayer != null && videoPlayer.length > 0.0001)
            progressBar.value = (float)(videoPlayer.time / videoPlayer.length) * 100f;

        if (cameraController != null)
            cameraController.canRotate = IsAnyModelPageActive();

    }

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
    // Camera rig (simple)
    // -------------------------
    private void BuildCameraRigBaseOnly()
    {
        if (cameraPosition == null || cameraPosition.Cornea == null) return;

        // Always ensure 0..3 exists
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

        // Fixed slots
        rig[CAM_SWOOP] = swoopPosition;
        rig[CAM_START] = startPosition;
        rig[CAM_MODEL_VIEW] = modelViewPosition;
        rig[CAM_VIDEO] = videoPosition;

        // Inspect slots
        for (int i = 0; i < inspectCount; i++)
        {
            var p = model.inspectPoints[i];
            rig[FIRST_DYNAMIC_CAM + i] = (p != null) ? p.cameraAnchor : null;
        }

        cameraPosition.Cornea.LerpCameraPositions = rig;
        activeInspectIndex = -1;
    }

    // =======================
    // Decal Controller
    // =======================
    private enum UIMode { Home, Video, Model }

    private void SetDecalForMode(UIMode mode)
    {
        if (decalController == null) return;

        bool wantDecal = (mode == UIMode.Model);

        // Match your old behavior: model mode = decal visible, otherwise hidden
        if (wantDecal && !decalController.IsOpaque)
            decalController.StartFadeInAndScaleUp();
        else if (!wantDecal && decalController.IsOpaque)
            decalController.StartFadeOutAndScaleDown();
    }

    // =======================
    // UXML swapping
    // =======================
    private void LoadUXML(VisualTreeAsset uxml)
    {
        if (uxml == null || uiDocument == null) return;

        uiDocument.visualTreeAsset = uxml;
        root = uiDocument.rootVisualElement;
        currentTree = uxml;

        CacheUI();
        BindButtons();
        BuildModelButtonsIfNeeded();
        PopulateCurrentPage();
        UIBlockerRaycast.Instance?.Recache();

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

            ["modelTCButton"] = () => OpenModelPage(0),
            ["specsTCButton"] = () => OpenModelPage(1),
            ["inspectTCButton"] = () => OpenModelPage(2),

            ["specsButton"] = () => OpenModelPage(1),
            ["inspectButton"] = () => OpenModelPage(2),
            ["viewModelsButton"] = () => OpenModelPage(0),
            ["viewSpecsButton"] = () => OpenModelPage(1),

            ["doneButton"] = GoHome,

            ["hide"] = ToggleUIVisibility,
            ["screenshot"] = TakeScreenshot,
        };
    }

    private void BindButtons()
    {
        if (actions == null || root == null) return;

        foreach (var kv in actions)
        {
            var btn = root.Q<Button>(kv.Key);
            if (btn == null) continue;

            btn.clicked -= kv.Value;
            btn.clicked += kv.Value;
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
        LoadUXML(homeUxml);
    }

    private void OpenVideo()
    {
        BuildCameraRigBaseOnly();
        ChangeVideoFOV(true);
        cameraPosition.goToPosition(CAM_VIDEO);
        SetRotationTargetActive(false);
        SetDecalForMode(UIMode.Video);
        LoadUXML(videoUxml); 
        if (playButton != null && videoPlayer != null && !videoPlayer.isPrepared) 
            playButton.SetEnabled(false);
    }

    private void OpenModelRoot()
    {
        if (!HasModelPages(1))
        {
            Debug.LogError("modelUxmlPages not assigned. Needs at least [0]=Models page.");
            return;
        }

        PauseVideo();
        ChangeVideoFOV(false);

        BuildCameraRigForModel(currentSimModel);

        cameraPosition.goToPosition(CAM_MODEL_VIEW);
        SetRotationTargetActive(true);

        if (cameraController != null)
            cameraController.SetTarget(rotationTarget, true);

        SetDecalForMode(UIMode.Model);
        LoadUXML(modelUxmlPages[0]);
    }

    private void OpenModelPage(int pageIndex)
    {
        if (!HasModelPages(pageIndex + 1))
        {
            Debug.LogError($"modelUxmlPages missing page index {pageIndex}. Assign 0=Models,1=Specs,2=Inspect.");
            return;
        }

        // Keep camera in model view for all model pages
       // BuildCameraRigForModel(currentSimModel);
       // cameraPosition.goToPosition(CAM_MODEL_VIEW);
       // SetRotationTargetActive(true);
        //SetDecalForMode(UIMode.Video);
        LoadUXML(modelUxmlPages[pageIndex]);
    }

    private bool HasModelPages(int minCount)
        => modelUxmlPages != null
           && modelUxmlPages.Length >= minCount
           && modelUxmlPages.Take(minCount).All(p => p != null);

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

        // rebuild camera rig to include this model’s inspect anchors
        BuildCameraRigForModel(currentSimModel);

        UpdateModelSelectionUI();
        PopulateCurrentPage();
    }

    // -------------------------
    // UI Cache + Dynamic lists
    // -------------------------
    private void CacheUI()
    {
        playButton = root?.Q<Button>("play-button");
        pauseButton = root?.Q<Button>("pause-button");
        muteButton = root?.Q<Button>("mute-button");
        unmuteButton = root?.Q<Button>("unmute-button");
        progressBar = root?.Q<ProgressBar>("progress-bar");

        modelsContainer = root?.Q<VisualElement>("Models");
        specsListContainer = root?.Q<VisualElement>("SpecsContainer");
        inspectListContainer = root?.Q<VisualElement>("InspectContainer");
        selectedModel = root?.Q<Label>("selectedModel");
    }

    private void BuildModelButtonsIfNeeded()
    {
        if (modelsContainer == null || modelButtonTemplate == null) return;

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
            btn.clicked += () => SelectSimModel(m.id);

            modelsContainer.Add(instance);
            modelButtons[m.id] = btn;
        }

        UpdateModelSelectionUI();
    }

    private void UpdateModelSelectionUI()
    {
        foreach (var kv in modelButtons)
            kv.Value.EnableInClassList("active", kv.Key == selectedSimModel);

        if (selectedModel != null && currentSimModel != null)
            selectedModel.text = currentSimModel.modelName;
    }

    private void PopulateCurrentPage()
    {
        UpdateModelSelectionUI();   

        if (currentSimModel == null) return;

        if (IsCurrent(modelUxmlPages, 1))
            PopulateSpecsUI(currentSimModel);

        if (IsCurrent(modelUxmlPages, 2))
            PopulateInspectUI(currentSimModel);
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

                        // Invert the displayed fill:
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

        if (model.inspectPoints == null) return;

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
            var b = inspectButtons[i];
            if (b == null) continue;

            b.EnableInClassList("active", cameraIndex == activeInspectIndex);
        }
    }

    // -------------------------
    // Helpers
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

    private void OnVideoPrepared(VideoPlayer source)
    {
        if (playButton != null)
            playButton.SetEnabled(true);
    }

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

    private bool IsCurrent(VisualTreeAsset[] arr, int i)
        => arr != null && i >= 0 && i < arr.Length && arr[i] != null
           && currentTree != null && currentTree == arr[i];

    private bool IsAnyModelPageActive()
        => IsCurrent(modelUxmlPages, 0) || IsCurrent(modelUxmlPages, 1) || IsCurrent(modelUxmlPages, 2);

    private void TakeScreenshot()
    {
        if (ScreenshotHandler.Instance != null)
            ScreenshotHandler.Instance.CaptureScreenshot();
    }

    private void ToggleUIVisibility()
    {
        var right = root?.Q<VisualElement>("RightContainer");
        if (right == null) return;

        uiHidden = !uiHidden;

        right.style.display = uiHidden
            ? DisplayStyle.None
            : DisplayStyle.Flex;
    }

}

