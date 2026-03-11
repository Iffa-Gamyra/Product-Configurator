using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class HomeScreenUI
{
    public VisualElement Root { get; }

    public VisualElement WelcomeScreen { get; }
    public VisualElement HomeScreen { get; }
    public VisualElement VideoScreen { get; }
    public VisualElement ModelSelectionScreen { get; }
    public VisualElement ModelSpecsScreen { get; }
    public VisualElement InspectModelScreen { get; }
    public VisualElement InfoOverlay { get; }
    public VisualElement BottomPanel { get; }

    public VisualElement ModelsContainer { get; }
    public VisualElement SpecsListContainer { get; }
    public VisualElement InspectListContainer { get; }

    public Label SelectedModelInSpecScreen { get; }
    public Label SelectedModelInInspectScreen { get; }

    public Button DownloadPdfButton { get; }
    public Button ResetViewButton { get; }
    public Button InspectPrevButton { get; }
    public Button InspectNextButton { get; }

    public Button PlayButton { get; }
    public Button PauseButton { get; }
    public Button MuteButton { get; }
    public Button UnmuteButton { get; }
    public Button ReplayButton { get; }

    public ProgressBar ProgressBar { get; }

    public List<VisualElement> AllScreens { get; }

    private readonly bool isMobileLayout;

    public HomeScreenUI(VisualElement root, bool isMobileLayout)
    {
        Root = root;
        this.isMobileLayout = isMobileLayout;

        WelcomeScreen = root.Q<VisualElement>("WelcomeScreen");
        HomeScreen = root.Q<VisualElement>("HomeScreen");
        VideoScreen = root.Q<VisualElement>("VideoScreen");
        ModelSelectionScreen = root.Q<VisualElement>("ModelSelection");
        ModelSpecsScreen = root.Q<VisualElement>("ModelSpecs");
        InspectModelScreen = root.Q<VisualElement>("InspectModel");
        InfoOverlay = root.Q<VisualElement>("InfoOverlay");
        BottomPanel = root.Q<VisualElement>("BottomPanel");

        PlayButton = root.Q<Button>("play-button");
        PauseButton = root.Q<Button>("pause-button");
        MuteButton = root.Q<Button>("mute-button");
        UnmuteButton = root.Q<Button>("unmute-button");
        ReplayButton = root.Q<Button>("replay-button");
        ProgressBar = root.Q<ProgressBar>("progress-bar");

        if (isMobileLayout)
        {
            ModelsContainer = HomeScreen?.Q<VisualElement>("Models");

            SpecsListContainer = HomeScreen?.Q<VisualElement>("SpecsContainer");
            DownloadPdfButton = HomeScreen?.Q<Button>("downloadButton");

            SelectedModelInSpecScreen = ModelSpecsScreen?.Q<Label>("selectedModel");
            SelectedModelInInspectScreen = InspectModelScreen?.Q<Label>("selectedModel");

            InspectListContainer = HomeScreen?.Q<VisualElement>("InspectContainer");
            ResetViewButton = HomeScreen?.Q<Button>("resetViewButton");
            InspectPrevButton = HomeScreen?.Q<Button>("inspectPrevButton");
            InspectNextButton = HomeScreen?.Q<Button>("inspectNextButton");
        }
        else
        {
            ModelsContainer = ModelSelectionScreen?.Q<VisualElement>("Models");

            SpecsListContainer = ModelSpecsScreen?.Q<VisualElement>("SpecsContainer");
            DownloadPdfButton = ModelSpecsScreen?.Q<Button>("downloadButton");
            SelectedModelInSpecScreen = ModelSpecsScreen?.Q<Label>("selectedModel");

            InspectListContainer = InspectModelScreen?.Q<VisualElement>("InspectContainer");
            SelectedModelInInspectScreen = InspectModelScreen?.Q<Label>("selectedModel");
            ResetViewButton = InspectModelScreen?.Q<Button>("resetViewButton");
            InspectPrevButton = InspectModelScreen?.Q<Button>("inspectPrevButton");
            InspectNextButton = InspectModelScreen?.Q<Button>("inspectNextButton");
        }

        if (ResetViewButton != null)
            ResetViewButton.style.display = DisplayStyle.None;

        if (InfoOverlay != null)
            InfoOverlay.style.display = DisplayStyle.None;

        AllScreens = BuildScreenList();
    }

    private List<VisualElement> BuildScreenList()
    {
        var screens = new List<VisualElement>(6);

        if (isMobileLayout)
        {
            screens.Add(WelcomeScreen);
            screens.Add(HomeScreen);
            screens.Add(VideoScreen);
        }
        else
        {
            screens.Add(WelcomeScreen);
            screens.Add(HomeScreen);
            screens.Add(VideoScreen);
            screens.Add(ModelSelectionScreen);
            screens.Add(ModelSpecsScreen);
            screens.Add(InspectModelScreen);
        }

        return screens;
    }

    public void BindButtons(Dictionary<string, Action> actions)
    {
        BindButtonsIn(WelcomeScreen, actions);
        BindButtonsIn(HomeScreen, actions);
        BindButtonsIn(VideoScreen, actions);
        BindButtonsIn(ModelSelectionScreen, actions);
        BindButtonsIn(ModelSpecsScreen, actions);
        BindButtonsIn(InspectModelScreen, actions);
        BindButtonsIn(InfoOverlay, actions);
    }

    private void BindButtonsIn(VisualElement scope, Dictionary<string, Action> actions)
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

    public void BindInfoOverlayButtons(Action toggleInfoOverlay)
    {
        if (Root == null || toggleInfoOverlay == null) return;

        Root.Query<Button>("infoButton").ForEach(b =>
        {
            b.clicked -= toggleInfoOverlay;
            b.clicked += toggleInfoOverlay;
        });

        var close = InfoOverlay?.Q<Button>("closeButton");
        if (close != null)
        {
            close.clicked -= toggleInfoOverlay;
            close.clicked += toggleInfoOverlay;
        }
    }
}