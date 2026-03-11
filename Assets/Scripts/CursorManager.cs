using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;

    private Vector2 cursorHotSpot;
    private bool isCustomCursorActive;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main; 
    }

    private void Start()
    {
        if (cursorTexture != null)
            cursorHotSpot = new Vector2(cursorTexture.width * 0.5f, cursorTexture.height * 0.5f);

        
        SetDefaultCursor();
    }

    private void Update()
    {
        
        var ui = UIBlockerRaycast.Instance;
        if (ui != null && ui.IsPointerOverUI())
        {
            if (isCustomCursorActive)
                SetDefaultCursor();
            return;
        }

        bool isHoldingLeftClick = Input.GetMouseButton(0);

        
        if (cam == null)
        {
            if (!isHoldingLeftClick && isCustomCursorActive)
                SetDefaultCursor();
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            bool hitCursor = hit.collider != null && hit.collider.CompareTag("cursor");

            if (hitCursor)
            {
                if (!isCustomCursorActive || !isHoldingLeftClick)
                    SetCustomCursor();
            }
            else if (!isHoldingLeftClick)
            {
                if (isCustomCursorActive)
                    SetDefaultCursor();
            }
        }
        else if (!isHoldingLeftClick)
        {
            if (isCustomCursorActive)
                SetDefaultCursor();
        }
    }

    private void SetCustomCursor()
    {
        if (isCustomCursorActive) return;
        Cursor.SetCursor(cursorTexture, cursorHotSpot, CursorMode.Auto);
        isCustomCursorActive = true;
    }

    private void SetDefaultCursor()
    {
        if (!isCustomCursorActive) return;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        isCustomCursorActive = false;
    }
}