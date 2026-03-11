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
    }

    public bool IsPointerOverUI()
    {
        Vector2 screenPos = Input.touchCount > 0
            ? Input.GetTouch(0).position
            : (Vector2)Input.mousePosition;

        return IsScreenPositionOverUI(screenPos);
    }

    public bool IsScreenPositionOverUI(Vector2 screenPos)
    {
        if (uiDocument == null) return false;

        var root = uiDocument.rootVisualElement;
        var panel = root?.panel;
        if (panel == null) return false;

        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, screenPos);
        VisualElement picked = panel.Pick(panelPos);

        if (picked == null || picked == root)
            return false;

        for (var ve = picked; ve != null; ve = ve.parent)
        {
            if (ve.name == blockerName)
                return true;

            if (ve.ClassListContains("ui-blocker"))
                return true;

            if (ve is Button || ve is ScrollView || ve is Scroller || ve is Slider)
                return true;
        }

        return false;
    }
}