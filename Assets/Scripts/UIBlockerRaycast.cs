using UnityEngine;
using UnityEngine.UIElements;

public class UIBlockerRaycast : MonoBehaviour
{
    public static UIBlockerRaycast Instance { get; private set; }

    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string blockerName = "UIBlocker";

    private VisualElement blocker;

    void Awake()
    {
        Instance = this;

        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        blocker = root.Q<VisualElement>(blockerName);

        if (blocker != null)
            blocker.pickingMode = PickingMode.Position;
    }

    public bool IsPointerOverBlocker()
    {
        if (uiDocument == null) return false;

        var panel = uiDocument.rootVisualElement?.panel;
        if (panel == null) return false;

        // Convert screen pixels to panel coordinates correctly (handles WebGL scaling)
        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, Input.mousePosition);

        var picked = panel.Pick(panelPos);
        if (picked == null) return false;

        // Name-based (your current approach)
        // return picked == blocker || (blocker != null && blocker.Contains(picked));

        // Better: class-based multi-blocker approach (recommended)
        for (var ve = picked; ve != null; ve = ve.parent)
            if (ve.ClassListContains("ui-blocker") || ve.name == blockerName)
                return true;

        return false;
    }

    public void Recache()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;
        blocker = root.Q<VisualElement>(blockerName);
        if (blocker != null) blocker.pickingMode = PickingMode.Position;
    }

}
