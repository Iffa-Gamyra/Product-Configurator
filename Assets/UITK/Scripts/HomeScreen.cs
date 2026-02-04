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
    public Transform bikeTarget;
    public Transform spawnPoint;

    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("UI Sections (ScriptableObjects)")]
    public UISection homeSection;
    public UISection videoSection;
    public UISection[] bikeSections;   // 4

    [Header("Bike Models (ScriptableObjects)")]
    public BikeModel modelA;
    public BikeModel modelB;

    [Header("GamyraDrive Models (ScriptableObjects)")]
    public SimulatorModel B1;
    public SimulatorModel C1;
    public SimulatorModel S1;
    public SimulatorModel L6;
    public SimulatorModel VR;
    public SimulatorModel MC;
    private SimulatorModel currentSimModel;
    public string selectedSimModel = "B1";   // default
    private Dictionary<string, SimulatorModel> simLookup;
    private GameObject currentModelInstance;             // the spawned model in the scene


    [Header("Cart Store")]
    public CartStore cartStore;

    [Header("Video")]
    public float video_FOV;
    public float normal_FOV;

    // ===== runtime =====
    private VisualElement root;          // DOC ROOT (old behavior)
    private VisualTreeAsset currentTree; // currently loaded tree

    private VideoPlayer videoPlayer;
    private Button playButton, pauseButton;
    private ProgressBar progressBar;

    private Label totalLabel;
    private Label modelTypeLabel, modelPriceLabel;
    private Label bodyColorLabel, bodyColorPriceLabel;
    private Label badgeColorLabel, badgePriceLabel;
    private Label seatColorLabel, seatPriceLabel;
    private Button alphaButton, deltaButton;

    private Dictionary<string, Action> actions;
    private Dictionary<string, Action> bikeActions;

    private readonly Dictionary<string, int> selections = new Dictionary<string, int>();

    // Flow state
    private string currentUI = "";
    private int bikeIndex = 0;


    // Focus state
    public bool isFocused = false;
    private int currentPosition = -1;

    // Bike/shirt state
    private BikeModel currentBikeModel;
    public string selectedModel = "Model A";


    // Cart keys
    private const string CART_MODEL = "Model";
    private const string CART_BODY = "Body";
    private const string CART_SEAT = "Seat";
    private const string CART_BADGE = "Badge";



    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();


        // Build lookup for sim selection
        simLookup = new Dictionary<string, SimulatorModel>(StringComparer.OrdinalIgnoreCase)
    {
        { B1.modelName, B1 },
        { C1.modelName, C1 },
        { S1.modelName, S1 },
        { L6.modelName, L6 },
        { VR.modelName, VR },
        { MC.modelName, MC },
    };
        BuildActions();
        BuildBikeActions();

    }
    void Start()
    {
        Debug.Log("HomeScreen Awake()");

        root = uiDocument.rootVisualElement;

        VisualElement welcomeScreen = root.Q("WelcomeScreen");
        VisualElement homeScreen = root.Q("HomeScreen");

        WelcomeScreenManager wsManager = new WelcomeScreenManager(welcomeScreen);
        wsManager.Start = () =>
        {
            cameraPosition.goToPosition(1);
            welcomeScreen.Display(false);
            homeScreen.Display(true);

            if (videoPlayer != null && !videoPlayer.isPrepared) videoPlayer.Prepare();
        };

        if (videoPlayer != null)
            videoPlayer.prepareCompleted += OnVideoPrepared;

        ChangeColorBody(0);
        ChangeColorBadge(0);
        ChangeColorSeat(0);
        SelectSimModel("B1");
    }
    private void OnEnable()
    {
        if (uiDocument == null) return;

        // Old behavior: bind against document root
        root = uiDocument.rootVisualElement;

        if (currentBikeModel == null)
            currentBikeModel = modelA;


        // If nothing loaded yet, assume we're on home
        if (currentTree == null && uiDocument.visualTreeAsset != null)
            currentTree = uiDocument.visualTreeAsset;

        CacheUI();
        BindButtons();
        RestoreState();
    }

    private void Update()
    {
        // progress bar (update even when paused)
        if (progressBar != null && videoPlayer != null && videoPlayer.length > 0.0001)
            progressBar.value = (float)(videoPlayer.time / videoPlayer.length) * 100f;

        if (cameraController == null || currentTree == null) return;

        bool isBike0 = IsCurrent(bikeSections, 0);
        bool isBike1 = IsCurrent(bikeSections, 1);
        bool isBike2 = IsCurrent(bikeSections, 2);
        bool isBike3 = IsCurrent(bikeSections, 3);

        if ((isBike0 || isBike1) && !isFocused) cameraController.canRotate = true;
        else if (isBike1 && isFocused) cameraController.canRotate = false;
        else if (!isBike1) { cameraController.canRotate = false; isFocused = false; }

        if ((isBike2 || isBike3) && !isFocused)
            cameraController.canRotate = true;
    }

    // =======================
    // UXML swapping (TRUE old-style rendering)
    // =======================

    private void LoadUXML(VisualTreeAsset uxml)
    {
        if (uxml == null || uiDocument == null) return;

        // CRITICAL: this fixes “rendering wrong” issues
        uiDocument.visualTreeAsset = uxml;

        // After changing visualTreeAsset, reacquire root
        root = uiDocument.rootVisualElement;
        currentTree = uxml;

        CacheUI();
        BindButtons();
        RestoreState();
    }

    private void GoHome()
    {
        ChangeCameraPosition(1);
        SetTargets(false, false);
        PauseVideo();
        ChangeVideoFOV("Normal");
        isFocused = false;
        currentPosition = -1;

        if (homeSection != null) LoadUXML(homeSection.uxmlAsset);
    }

    // =======================
    // Actions (IDs unchanged, but split)
    // =======================

    private void BuildActions()
    {
        actions = new Dictionary<string, Action>
        {
            ["home"] = () => { GoHome(); HandleButtonClick("bike"); },

            ["video"] = () => { HandleButtonClick("video"); OpenVideo(); },
            ["video_tab"] = () => { HandleButtonClick("video_tab"); OpenVideo(); },

            ["play-button"] = PlayVideo,
            ["pause-button"] = PauseVideo,

            ["screenshot"] = TakeScreenshot,


        };
    }
    private void BuildBikeActions()
    {
        bikeActions = new Dictionary<string, Action>
        {
            ["bike"] = () => { HandleButtonClick("bike"); OpenBikeRoot(); },
            ["bike_tab"] = () => { HandleButtonClick("bike_tab"); OpenBikeRoot(); },

            ["alpha"] = () => { currentBikeModel = modelA; SelectModel("Model A"); SwitchSpecs("Model A"); },
            ["delta"] = () => { currentBikeModel = modelB; SelectModel("Model B"); SwitchSpecs("Model B"); },

            // 6 simulator selections
            ["B1"] = () => SelectSimModel("B1"),
            ["C1"] = () => SelectSimModel("C1"),
            ["S1"] = () => SelectSimModel("S1"),
            ["L6"] = () => SelectSimModel("L6"),
            ["VR"] = () => SelectSimModel("VR"),
            ["MC"] = () => SelectSimModel("MC"),


            ["Customisation_tab"] = () => { bikeIndex = 1; LoadUXML(bikeSections[1].uxmlAsset); },
            ["Customisation_tab_delta"] = () => { bikeIndex = 1; LoadUXML(bikeSections[2].uxmlAsset); },

            ["next_bike"] = () =>
            {
                if (bikeSections != null && bikeIndex < bikeSections.Length - 1)
                    LoadUXML(bikeSections[++bikeIndex].uxmlAsset);
            },
            ["back_bike"] = () =>
            {
                if (bikeIndex > 0) LoadUXML(bikeSections[--bikeIndex].uxmlAsset);
                SetTargets(true, false);
                if (cameraController != null) cameraController.SetTarget(bikeTarget, true);
            },

            ["cart_tab"] = () =>
            {
                bikeIndex = 2;
                LoadUXML(bikeSections[2].uxmlAsset);
                SetTargets(true, false);
                if (cameraController != null) cameraController.SetTarget(bikeTarget, true);

            },
            ["reserve"] = () => LoadUXML(bikeSections[3].uxmlAsset),
            ["reserve_tab"] = () => { bikeIndex = 3; LoadUXML(bikeSections[3].uxmlAsset); },
            ["confirm_bike"] = GoHome,

            ["body_white"] = () => ChangeColorBody(0),
            ["body_silver"] = () => ChangeColorBody(1),
            ["body_alumin"] = () => ChangeColorBody(2),
            ["body_carbon"] = () => ChangeColorBody(3),
            ["body_black"] = () => ChangeColorBody(4),

            ["badge_silver"] = () => ChangeColorBadge(0),
            ["badge_black"] = () => ChangeColorBadge(1),

            ["seat_tan"] = () => ChangeColorSeat(0),
            ["seat_black"] = () => ChangeColorSeat(1),

            ["inspect_body"] = () => { Inspect(3, "body"); SetTargets(false, false); },
            ["inspect_badge"] = () => { Inspect(5, "badge"); SetTargets(false, false); },
            ["inspect_seat"] = () => { Inspect(4, "seat"); SetTargets(false, false); },
        };
    }


    private void BindButtons()
    {
        foreach (var kv in actions)
        {
            var btn = root.Q<Button>(kv.Key);
            if (btn == null) continue;

            btn.clicked -= kv.Value;
            btn.clicked += kv.Value;
        }
        foreach (var kv in bikeActions)
        {
            var btn = root.Q<Button>(kv.Key);
            if (btn == null) continue;

            btn.clicked -= kv.Value;
            btn.clicked += kv.Value;
        }
    }

    private void CacheUI()
    {
        playButton = root.Q<Button>("play-button");
        pauseButton = root.Q<Button>("pause-button");
        progressBar = root.Q<ProgressBar>("progress-bar");

        alphaButton = root.Q<Button>("alpha");
        deltaButton = root.Q<Button>("delta");

        modelTypeLabel = root.Q<Label>("Model_Type");
        modelPriceLabel = root.Q<Label>("Model_Price");

        bodyColorLabel = root.Q<Label>("Body_Color");
        bodyColorPriceLabel = root.Q<Label>("Body_Color_Price");

        badgeColorLabel = root.Q<Label>("Badge_Color");
        badgePriceLabel = root.Q<Label>("Badge_Price");

        seatColorLabel = root.Q<Label>("Seat_Color");
        seatPriceLabel = root.Q<Label>("Seat_Price");

        totalLabel = root.Q<Label>("TotalLabel");
    }

    private void RestoreState()
    {

        if (selections.TryGetValue("body", out var b)) ChangeColorBody(b);
        if (selections.TryGetValue("badge", out var ba)) ChangeColorBadge(ba);
        if (selections.TryGetValue("seat", out var se)) ChangeColorSeat(se);

        //SelectModel(selectedModel);
        SelectSimModel(selectedSimModel);
        UpdateTotalLabel();
        SwitchSpecs(selectedModel);
        UpdateTotalLabel();
    }

    private void OpenVideo()
    {
        ChangeVideoFOV("Video");
        ChangeCameraPosition(6);
        SetTargets(false, false);
        if (videoSection != null) LoadUXML(videoSection.uxmlAsset);
        // disable play button until ready
        if (playButton != null && !videoPlayer.isPrepared)
            playButton.SetEnabled(false);
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        if (playButton != null)
            playButton.SetEnabled(true);
    }

    private void OpenBikeRoot()
    {
        Debug.Log("OpenBikeRoot() called");
        PauseVideo();
        ChangeVideoFOV("Normal");
        ChangeCameraPosition(2);
        SetTargets(true, false);
        if (cameraController != null) cameraController.SetTarget(bikeTarget, true);

        bikeIndex = 0;
        if (bikeSections != null && bikeSections.Length > 0) LoadUXML(bikeSections[0].uxmlAsset);
    }

    // =======================
    // CartStore-driven selections
    // =======================

    private void SelectSimModel(string name)
    {
        selectedSimModel = name;
        Debug.Log($"SelectSimModel({name}) called");
        // Resolve which ScriptableObject to use
        switch (name)
        {
            case "B1": currentSimModel = B1; break;
            case "C1": currentSimModel = C1; break;
            case "S1": currentSimModel = S1; break;
            case "L6": currentSimModel = L6; break;
            case "VR": currentSimModel = VR; break;
            case "MC": currentSimModel = MC; break;
            default: currentSimModel = B1; break;
        }

        if (currentSimModel == null || currentSimModel.model == null) return;

        // Destroy previous spawned sim
        if (currentModelInstance != null)
            Destroy(currentModelInstance);

        // Spawn selected sim
        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = currentSimModel.model.transform.rotation;

        currentModelInstance = Instantiate(currentSimModel.model, pos, rot);
        Debug.Log($"Spawning at {(spawnPoint ? spawnPoint.position : Vector3.zero)}");

        // UI + cart
        if (modelTypeLabel != null) modelTypeLabel.text = currentSimModel.modelName;
        if (modelPriceLabel != null) modelPriceLabel.text = $"${currentSimModel.basePrice:N0}";
        if (cartStore != null) cartStore.SetItem(CART_MODEL, currentSimModel.basePrice);

        UpdateTotalLabel();
    }


    private void SelectModel(string name)
    {
        selectedModel = name;
        currentBikeModel = (name == "Model B") ? modelB : modelA;
        if (currentBikeModel == null) return;

        // remove previous spawned bike (if any)
        if (currentModelInstance != null)
            Destroy(currentModelInstance);

        // spawn the selected model prefab
        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = currentBikeModel.model.transform.rotation;

        currentModelInstance = Instantiate(currentBikeModel.model, pos, rot);


        if (modelTypeLabel != null) modelTypeLabel.text = currentBikeModel.modelName;
        if (modelPriceLabel != null) modelPriceLabel.text = $"${currentBikeModel.basePrice:N0}";
        if (cartStore != null) cartStore.SetItem(CART_MODEL, currentBikeModel.basePrice);
    }

    private void ChangeColorBody(int i)
    {
        var opt = GetOpt(currentBikeModel?.bodyColors, i); if (opt == null) return;
        ApplyMaterialByTag("body", opt.material);
        if (bodyColorLabel != null) bodyColorLabel.text = opt.displayName;
        if (bodyColorPriceLabel != null) bodyColorPriceLabel.text = $"${opt.price:N0}";
        if (cartStore != null) cartStore.SetItem(CART_BODY, opt.price);
        HighlightButtons(new[] { "body_white", "body_silver", "body_alumin", "body_carbon", "body_black" }, i, "body");
    }

    private void ChangeColorSeat(int i)
    {
        var opt = GetOpt(currentBikeModel?.seatColors, i); if (opt == null) return;
        ApplyMaterialByTag("seat", opt.material);
        if (seatColorLabel != null) seatColorLabel.text = opt.displayName;
        if (seatPriceLabel != null) seatPriceLabel.text = $"${opt.price:N0}";
        if(cartStore!=null) cartStore.SetItem(CART_SEAT,opt.price);
        HighlightButtons(new[] { "seat_tan", "seat_black" }, i, "seat");
    }

    private void ChangeColorBadge(int i)
    {
        var opt = GetOpt(currentBikeModel?.badgeColors, i); if (opt == null) return;
        ApplyMaterialByTag("badge", opt.material);
        if (badgeColorLabel != null) badgeColorLabel.text = opt.displayName;
        if (badgePriceLabel != null) badgePriceLabel.text = $"${opt.price:N0}";
        if (cartStore != null) cartStore.SetItem(CART_BADGE, opt.price);
        HighlightButtons(new[] { "badge_silver", "badge_black" }, i, "badge");
    }


    private void UpdateTotalLabel()
    {
        if (totalLabel == null) return;
        if (cartStore == null) { totalLabel.text = "$0"; return; }
        cartStore.RecalculateTotal();
        totalLabel.text = $"${cartStore.totalPrice:N0}";
    }

    private void SwitchSpecs(string model)
    {
        var specsAlpha = root.Q("Specs_Alpha");
        var specsDelta = root.Q("Specs_Delta");
        if (alphaButton == null || deltaButton == null) return;

        bool isA = (model == "Model A");
        if (specsAlpha != null) specsAlpha.style.display = isA ? DisplayStyle.Flex : DisplayStyle.None;
        if (specsDelta != null) specsDelta.style.display = isA ? DisplayStyle.None : DisplayStyle.Flex;

        SetModelButtonStyle(alphaButton, isA);
        SetModelButtonStyle(deltaButton, !isA);
    }

    private void SetModelButtonStyle(Button button, bool isSelected)
    {
        if (button == null) return;
        if (isSelected)
        {
            button.style.backgroundColor = new StyleColor(new Color32(146, 242, 225, 255));
            button.style.color = new StyleColor(Color.black);
        }
        else
        {
            button.style.backgroundColor = new StyleColor(new Color32(0, 0, 0, 150));
            button.style.color = new StyleColor(Color.white);
        }
    }

    private void HighlightButtons(string[] buttonIds, int selectedIndex, string group)
    {
        Color32 selectedBorderColor = new Color32(0, 190, 250, 255);

        for (int i = 0; i < buttonIds.Length; i++)
        {
            var button = root.Q<Button>(buttonIds[i]);
            if (button == null) continue;

            if (i == selectedIndex)
            {
                button.style.borderBottomWidth = 2;
                button.style.borderBottomColor = new StyleColor(selectedBorderColor);
            }
            else button.style.borderBottomWidth = 0;
        }

        selections[group] = selectedIndex;
    }

    private void Inspect(int pos, string part)
    {
        var inspectButtons = new[]
        {
            root.Q<Button>("inspect_body"),
            root.Q<Button>("inspect_badge"),
            root.Q<Button>("inspect_seat")
        };

        if (currentPosition == pos) isFocused = !isFocused;
        else isFocused = true;

        if (isFocused)
        {
            foreach (var btn in inspectButtons)
            {
                if (btn == null) continue;

                if (btn.name == $"inspect_{part}")
                {
                    btn.style.unityBackgroundImageTintColor = new StyleColor(new Color(0.9137255f, 0.49411766f, 0f, 1f));
                    if(cameraPosition != null) cameraPosition.goToPosition(pos);
                    currentPosition = pos;
                }
                else btn.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
            }
        }
        else
        {
            foreach (var btn in inspectButtons)
                if (btn != null) btn.style.unityBackgroundImageTintColor = new StyleColor(Color.white);

            if(cameraPosition!=null) cameraPosition.goToPosition(2);
            currentPosition = -1;
        }
    }

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

    private void ChangeVideoFOV(string state)
    {
        if (Camera.main == null) return;
        Camera.main.fieldOfView = (state == "Video") ? video_FOV : normal_FOV;
    }

    private void ChangeCameraPosition(int pos)
    {
        if (cameraPosition != null) cameraPosition.goToPosition(pos);
    }

    private void SetTargets(bool bikeOn, bool merchOn)
    {
        if (bikeTarget) bikeTarget.gameObject.SetActive(bikeOn);
    }

    private void HandleButtonClick(string uiName)
    {
        if (currentUI == uiName) return;

        var decalUIList = new HashSet<string> { "bike", "bike_tab", "next_bike" };

        if (decalController != null)
        {
            if (decalUIList.Contains(currentUI) && decalController.IsOpaque)
                decalController.StartFadeOutAndScaleDown();

            if (decalUIList.Contains(uiName) && !decalController.IsOpaque)
                decalController.StartFadeInAndScaleUp();
        }

        currentUI = uiName;
    }

    private void TakeScreenshot()
    {
        if (ScreenshotHandler.Instance != null)
            ScreenshotHandler.Instance.CaptureScreenshot();
    }

    private bool IsCurrent(UISection[] arr, int i)
        => arr != null && i >= 0 && i < arr.Length && arr[i] != null && arr[i].uxmlAsset != null
           && currentTree != null && currentTree.name == arr[i].uxmlAsset.name;

    private static ColorOption GetOpt(ColorOption[] arr, int i)
        => (arr != null && i >= 0 && i < arr.Length) ? arr[i] : null;

    private static void ApplyMaterialByTag(string tag, Material mat)
    {
        if (mat == null) return;
        var go = GameObject.FindWithTag(tag);
        if (go == null) return;
        var mr = go.GetComponent<MeshRenderer>();
        if (mr == null) return;
        mr.material = mat;
    }
}
