using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class HomeScreenUI
{
    public VisualElement Root { get; }
    public VisualElement WelcomeScreen { get; }
    public VisualElement HomeScreen { get; }
    public VisualElement VideoScreen { get; }
    public VisualElement ProductSelectionScreen { get; }
    public VisualElement ProductSpecsScreen { get; }
    public VisualElement InspectProductScreen { get; }
    public VisualElement InfoOverlay { get; }
    public VisualElement BottomPanel { get; }

    public Label WelcomeTitleLabel { get; }
    public Label WelcomeDescLabel { get; }
    public Button WelcomeStartBtn { get; }

    public Button HomeProductTabBtn { get; private set; }
    public Button HomeVideoTabBtn { get; private set; }
    public Label HomeLogoLabel { get; private set; }

    public List<Button> SideNavHomeBtns { get; private set; }
    public List<Button> SideNavProductBtns { get; private set; }
    public List<Button> SideNavVideoBtns { get; private set; }
    public List<Button> SideNavInfoBtns { get; private set; }

    public List<Button> MobileHomeTabBtns { get; private set; }
    public List<Button> MobileVideoTabBtns { get; private set; }

    public List<VisualElement> TopNavContainers { get; private set; }
    public List<VisualElement> MobileNavContainers { get; private set; }
    public List<Label> BannerLabels { get; private set; }
    public List<VisualElement> RightContainers { get; private set; }

    public Button UtilsHideBtn { get; private set; }
    public Button UtilsScreenshotBtn { get; private set; }
    public Label UtilsLogoLabel { get; private set; }

    public VisualElement ProductsContainer { get; private set; }

    public VisualElement SpecsListContainer { get; private set; }
    public VisualElement SpecsSectionRoot { get; private set; }
    public VisualElement BrochureSectionRoot { get; private set; }
    public Button SpecsButton { get; private set; }
    public Button InspectButton { get; private set; }
    public Button InspectDoneBtn { get; private set; }
    public Label SpecsTitleLabel { get; private set; }
    public Label SpecsBackNavLabel { get; private set; }
    public Label SpecsDownloadLabel { get; private set; }
    public Button DownloadPdfButton { get; private set; }
    public Label SelectedProductInSpecScreen { get; private set; }

    public List<Button> SpecsTabButtons { get; private set; }
    public List<Button> InspectTabButtons { get; private set; }

    public VisualElement InspectListContainer { get; private set; }
    public VisualElement InspectSectionRoot { get; private set; }
    public Label InspectTitleLabel { get; private set; }
    public Label InspectBackNavLabel { get; private set; }
    public Button ResetViewButton { get; private set; }
    public Label InspectResetIcon { get; private set; }
    public Label InspectResetLabel { get; private set; }
    public Button InspectPrevButton { get; private set; }
    public Label InspectPrevLabel { get; private set; }
    public Button InspectNextButton { get; private set; }
    public Label InspectNextLabel { get; private set; }
    public Label SelectedProductInInspectScreen { get; private set; }

    public VisualElement InfoCard { get; private set; }
    public Label InfoTitleLabel { get; private set; }
    public Label InfoBodyLabel { get; private set; }
    public Button InfoCloseBtn { get; private set; }

    public Button PlayButton { get; }
    public Button PauseButton { get; }
    public Button MuteButton { get; }
    public Button UnmuteButton { get; }
    public Button ReplayButton { get; }
    public ProgressBar ProgressBar { get; }
    public VisualElement ProgressBarFill { get; private set; }
    public VisualElement ProgressBarTrack { get; private set; }

    public StyleTargets Targets { get; private set; }

    public List<VisualElement> AllScreens { get; }

    private readonly bool isMobileLayout;

    public HomeScreenUI(VisualElement root, bool isMobileLayout)
    {
        Root = root;
        this.isMobileLayout = isMobileLayout;

        WelcomeScreen = root.Q<VisualElement>(UINames.Screen_Welcome);
        HomeScreen = root.Q<VisualElement>(UINames.Screen_Home);
        VideoScreen = root.Q<VisualElement>(UINames.Screen_Video);
        ProductSelectionScreen = root.Q<VisualElement>(UINames.Screen_Products);
        ProductSpecsScreen = root.Q<VisualElement>(UINames.Screen_Specs);
        InspectProductScreen = root.Q<VisualElement>(UINames.Screen_Inspect);
        InfoOverlay = root.Q<VisualElement>(UINames.Overlay_Info);
        BottomPanel = root.Q<VisualElement>(UINames.Home_BottomPanel);

        WelcomeTitleLabel = root.Q<Label>(UINames.Welcome_Title);
        WelcomeDescLabel = root.Q<Label>(UINames.Welcome_Description);
        WelcomeStartBtn = root.Q<Button>(UINames.Welcome_StartBtn);

        PlayButton = root.Q<Button>(UINames.Video_PlayBtn);
        PauseButton = root.Q<Button>(UINames.Video_PauseBtn);
        MuteButton = root.Q<Button>(UINames.Video_MuteBtn);
        UnmuteButton = root.Q<Button>(UINames.Video_UnmuteBtn);
        ReplayButton = root.Q<Button>(UINames.Video_ReplayBtn);
        ProgressBar = root.Q<ProgressBar>(UINames.Video_ProgressBar);

        if (ProgressBar != null)
        {
            ProgressBarFill = ProgressBar.Q<VisualElement>(UINames.ProgressBar_Fill);
            ProgressBarTrack = ProgressBar.Q<VisualElement>(UINames.ProgressBar_Track);
        }

        InfoCard = root.Q<VisualElement>(UINames.Info_Card);
        InfoTitleLabel = root.Q<Label>(UINames.Info_TitleLabel);
        InfoBodyLabel = root.Q<Label>(UINames.Info_BodyLabel);
        InfoCloseBtn = root.Q<Button>(UINames.Info_CloseBtn);

        if (isMobileLayout) BindMobile();
        else BindDesktop();

        if (ResetViewButton != null)
            ResetViewButton.style.display = DisplayStyle.None;
        if (InfoOverlay != null)
            InfoOverlay.style.display = DisplayStyle.None;

        Targets = new StyleTargets(root);
        AllScreens = BuildScreenList();
    }

    private void BindMobile()
    {
        ProductsContainer = HomeScreen?.Q<VisualElement>(UINames.Mobile_ProductsList);

        SpecsListContainer = HomeScreen?.Q<VisualElement>(UINames.Mobile_SpecsContainer);
        DownloadPdfButton = HomeScreen?.Q<Button>(UINames.Specs_DownloadBtn);
        SpecsDownloadLabel = HomeScreen?.Q<Label>(UINames.Specs_DownloadLabel);
        SelectedProductInSpecScreen = HomeScreen?.Q<Label>(UINames.Mobile_SelectedProduct);

        SpecsButton = null;
        InspectButton = null;
        InspectDoneBtn = null;

        SpecsTabButtons = new List<Button>(0);
        InspectTabButtons = new List<Button>(0);

        MobileHomeTabBtns = new List<Button>();
        MobileVideoTabBtns = new List<Button>();
        Root.Query<Button>(UINames.MobileTopNav_Home).ForEach(b => MobileHomeTabBtns.Add(b));
        Root.Query<Button>(UINames.MobileTopNav_Video).ForEach(b => MobileVideoTabBtns.Add(b));

        MobileNavContainers = new List<VisualElement>();
        Root.Query<VisualElement>(UINames.MobileTopNav_Container)
            .ForEach(e => MobileNavContainers.Add(e));

        TopNavContainers = new List<VisualElement>(0);
        BannerLabels = new List<Label>(0);
        RightContainers = new List<VisualElement>(0);

        SpecsSectionRoot = HomeScreen?.Q<VisualElement>(UINames.Mobile_SpecsSection);
        InspectSectionRoot = HomeScreen?.Q<VisualElement>(UINames.Mobile_InspectSection);
        BrochureSectionRoot = HomeScreen?.Q<VisualElement>(UINames.Mobile_BrochureSection);

        InspectListContainer = HomeScreen?.Q<VisualElement>(UINames.Mobile_InspectContainer);
        ResetViewButton = HomeScreen?.Q<Button>(UINames.Inspect_ResetBtn);
        InspectResetIcon = HomeScreen?.Q<Label>(UINames.Inspect_ResetIcon);
        InspectResetLabel = HomeScreen?.Q<Label>(UINames.Inspect_ResetLabel);
        InspectPrevButton = HomeScreen?.Q<Button>(UINames.Inspect_PrevBtn);
        InspectPrevLabel = HomeScreen?.Q<Label>(UINames.Inspect_PrevLabel);
        InspectNextButton = HomeScreen?.Q<Button>(UINames.Inspect_NextBtn);
        InspectNextLabel = HomeScreen?.Q<Label>(UINames.Inspect_NextLabel);
        SelectedProductInInspectScreen = HomeScreen?.Q<Label>(UINames.Mobile_SelectedProduct);

        SpecsTitleLabel = HomeScreen?.Q<Label>(UINames.Specs_Title);
        InspectTitleLabel = HomeScreen?.Q<Label>(UINames.Inspect_Title);
        InspectBackNavLabel = HomeScreen?.Q<Label>(UINames.Inspect_BackNavLabel);
        SpecsBackNavLabel = HomeScreen?.Q<Label>(UINames.Specs_BackNavLabel);

        UtilsLogoLabel = HomeScreen?.Q<Label>(UINames.Utils_Logo);
        UtilsHideBtn = HomeScreen?.Q<Button>(UINames.Utils_Hide);
        UtilsScreenshotBtn = HomeScreen?.Q<Button>(UINames.Utils_Screenshot);
        HomeLogoLabel = HomeScreen?.Q<Label>(UINames.Home_LogoLabel);
        HomeProductTabBtn = HomeScreen?.Q<Button>(UINames.Home_ProductTabBtn);
        HomeVideoTabBtn = HomeScreen?.Q<Button>(UINames.Home_VideoTabBtn);

        SideNavHomeBtns = new List<Button>(0);
        SideNavProductBtns = new List<Button>(0);
        SideNavVideoBtns = new List<Button>(0);
        SideNavInfoBtns = new List<Button>();
        Root.Query<Button>(UINames.SideNav_Info).ForEach(b => SideNavInfoBtns.Add(b));
    }

    private void BindDesktop()
    {
        ProductsContainer = ProductSelectionScreen?.Q<VisualElement>(UINames.Products_List);

        SpecsListContainer = ProductSpecsScreen?.Q<VisualElement>(UINames.Specs_Container);
        DownloadPdfButton = ProductSpecsScreen?.Q<Button>(UINames.Specs_DownloadBtn);
        SpecsDownloadLabel = ProductSpecsScreen?.Q<Label>(UINames.Specs_DownloadLabel);
        SelectedProductInSpecScreen = ProductSpecsScreen?.Q<Label>(UINames.Specs_SelectedLabel);

        SpecsButton = ProductSelectionScreen?.Q<Button>(UINames.Products_NextBtn);
        InspectButton = ProductSpecsScreen?.Q<Button>(UINames.Specs_NextBtn);
        InspectDoneBtn = InspectProductScreen?.Q<Button>(UINames.Inspect_DoneBtn);

        SpecsTabButtons = new List<Button>(3);
        InspectTabButtons = new List<Button>(3);
        var productScreens = new[] { ProductSelectionScreen, ProductSpecsScreen, InspectProductScreen };
        foreach (var screen in productScreens)
        {
            if (screen == null) continue;
            var s = screen.Q<Button>(UINames.TopNav_Specs);
            var ins = screen.Q<Button>(UINames.TopNav_Inspect);
            if (s != null) SpecsTabButtons.Add(s);
            if (ins != null) InspectTabButtons.Add(ins);
        }

        TopNavContainers = new List<VisualElement>(3);
        var tnProd = Root.Q<VisualElement>(UINames.TopNav_ProductContainer);
        var tnSpec = Root.Q<VisualElement>(UINames.TopNav_SpecsContainer);
        var tnInsp = Root.Q<VisualElement>(UINames.TopNav_InspectContainer);
        if (tnProd != null) TopNavContainers.Add(tnProd);
        if (tnSpec != null) TopNavContainers.Add(tnSpec);
        if (tnInsp != null) TopNavContainers.Add(tnInsp);

        BannerLabels = new List<Label>();
        Root.Query<Label>().Where(l => l.ClassListContains(UINames.Class_BannerLabel))
            .ForEach(l => BannerLabels.Add(l));

        RightContainers = new List<VisualElement>();
        Root.Query<VisualElement>(UINames.RightContainer)
            .ForEach(e => RightContainers.Add(e));

        MobileNavContainers = new List<VisualElement>(0);
        MobileHomeTabBtns = new List<Button>(0);
        MobileVideoTabBtns = new List<Button>(0);

        SpecsSectionRoot = ProductSpecsScreen?.Q<VisualElement>(UINames.Specs_PanelCard);
        InspectSectionRoot = InspectProductScreen?.Q<VisualElement>(UINames.Inspect_PanelCard);
        BrochureSectionRoot = ProductSpecsScreen?.Q<VisualElement>(UINames.Specs_BrochureRoot);

        InspectListContainer = InspectProductScreen?.Q<VisualElement>(UINames.Inspect_Container);
        SelectedProductInInspectScreen = InspectProductScreen?.Q<Label>(UINames.Specs_SelectedLabel);
        ResetViewButton = InspectProductScreen?.Q<Button>(UINames.Inspect_ResetBtn);
        InspectResetIcon = InspectProductScreen?.Q<Label>(UINames.Inspect_ResetIcon);
        InspectResetLabel = InspectProductScreen?.Q<Label>(UINames.Inspect_ResetLabel);
        InspectPrevButton = InspectProductScreen?.Q<Button>(UINames.Inspect_PrevBtn);
        InspectPrevLabel = InspectProductScreen?.Q<Label>(UINames.Inspect_PrevLabel);
        InspectNextButton = InspectProductScreen?.Q<Button>(UINames.Inspect_NextBtn);
        InspectNextLabel = InspectProductScreen?.Q<Label>(UINames.Inspect_NextLabel);

        SpecsTitleLabel = ProductSpecsScreen?.Q<Label>(UINames.Specs_Title);
        InspectTitleLabel = InspectProductScreen?.Q<Label>(UINames.Inspect_Title);
        InspectBackNavLabel = InspectProductScreen?.Q<Label>(UINames.Inspect_BackNavLabel);
        SpecsBackNavLabel = ProductSpecsScreen?.Q<Label>(UINames.Specs_BackNavLabel);

        UtilsLogoLabel = Root.Q<Label>(UINames.Utils_Logo);
        UtilsHideBtn = Root.Q<Button>(UINames.Utils_Hide);
        UtilsScreenshotBtn = Root.Q<Button>(UINames.Utils_Screenshot);
        HomeLogoLabel = Root.Q<Label>(UINames.Home_LogoLabel);
        HomeProductTabBtn = HomeScreen?.Q<Button>(UINames.Home_ProductTabBtn);
        HomeVideoTabBtn = HomeScreen?.Q<Button>(UINames.Home_VideoTabBtn);

        SideNavHomeBtns = new List<Button>();
        SideNavProductBtns = new List<Button>();
        SideNavVideoBtns = new List<Button>();
        SideNavInfoBtns = new List<Button>();
        Root.Query<Button>(UINames.SideNav_Home).ForEach(b => SideNavHomeBtns.Add(b));
        Root.Query<Button>(UINames.SideNav_Product).ForEach(b => SideNavProductBtns.Add(b));
        Root.Query<Button>(UINames.SideNav_Video).ForEach(b => SideNavVideoBtns.Add(b));
        Root.Query<Button>(UINames.SideNav_Info).ForEach(b => SideNavInfoBtns.Add(b));
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
            screens.Add(ProductSelectionScreen);
            screens.Add(ProductSpecsScreen);
            screens.Add(InspectProductScreen);
        }
        return screens;
    }

    public void BindButtons(System.Collections.Generic.Dictionary<string, Action> actions)
    {
        BindIn(WelcomeScreen, actions);
        BindIn(HomeScreen, actions);
        BindIn(VideoScreen, actions);
        BindIn(ProductSelectionScreen, actions);
        BindIn(ProductSpecsScreen, actions);
        BindIn(InspectProductScreen, actions);
        BindIn(InfoOverlay, actions);
    }

    private static void BindIn(
        VisualElement scope,
        System.Collections.Generic.Dictionary<string, Action> actions)
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

    public void BindInfoOverlayButtons(Action toggle)
    {
        if (Root == null || toggle == null) return;
        foreach (var b in SideNavInfoBtns)
        {
            b.clicked -= toggle;
            b.clicked += toggle;
        }
        if (InfoCloseBtn != null)
        {
            InfoCloseBtn.clicked -= toggle;
            InfoCloseBtn.clicked += toggle;
        }
    }

    public class StyleTargets
    {
        public readonly List<VisualElement> FontBody = new();
        public readonly List<VisualElement> FontBold = new();
        public readonly List<VisualElement> FontLight = new();

        public readonly List<VisualElement> ColorPrimary = new();
        public readonly List<VisualElement> ColorBrand = new();
        public readonly List<VisualElement> ColorAccent = new();
        public readonly List<VisualElement> ColorMuted = new();
        public readonly List<VisualElement> ColorDark = new();

        public readonly List<VisualElement> TextXS = new();
        public readonly List<VisualElement> TextSM = new();
        public readonly List<VisualElement> TextMD = new();
        public readonly List<VisualElement> TextBase = new();
        public readonly List<VisualElement> TextTitle = new();
        public readonly List<VisualElement> TextLG = new();
        public readonly List<VisualElement> TextXL = new();
        public readonly List<VisualElement> Text2XL = new();

        public readonly List<VisualElement> Dividers = new();

        public StyleTargets(VisualElement root)
        {
            root.Query<VisualElement>().ForEach(e =>
            {
                if (e.ClassListContains(UINames.Font_Body)) FontBody.Add(e);
                if (e.ClassListContains(UINames.Font_Bold)) FontBold.Add(e);
                if (e.ClassListContains(UINames.Font_Light)) FontLight.Add(e);

                if (e.ClassListContains(UINames.TextColor_Primary)) ColorPrimary.Add(e);
                if (e.ClassListContains(UINames.TextColor_Brand)) ColorBrand.Add(e);
                if (e.ClassListContains(UINames.TextColor_Accent)) ColorAccent.Add(e);
                if (e.ClassListContains(UINames.TextColor_Muted)) ColorMuted.Add(e);
                if (e.ClassListContains(UINames.TextColor_Dark)) ColorDark.Add(e);

                if (e.ClassListContains(UINames.Text_XS)) TextXS.Add(e);
                if (e.ClassListContains(UINames.Text_SM)) TextSM.Add(e);
                if (e.ClassListContains(UINames.Text_MD)) TextMD.Add(e);
                if (e.ClassListContains(UINames.Text_Base)) TextBase.Add(e);
                if (e.ClassListContains(UINames.Text_Title)) TextTitle.Add(e);
                if (e.ClassListContains(UINames.Text_LG)) TextLG.Add(e);
                if (e.ClassListContains(UINames.Text_XL)) TextXL.Add(e);
                if (e.ClassListContains(UINames.Text_2XL)) Text2XL.Add(e);

                if (e.ClassListContains(UINames.Class_Divider) ||
                    e.ClassListContains(UINames.Class_DividerThin))
                    Dividers.Add(e);
            });
        }
    }
}
