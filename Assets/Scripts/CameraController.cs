using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [System.Serializable]
    public struct TransformStatus
    {
        public Transform targetTransform; // The transform to rotate around
        public bool rotateCamera;         // If true, rotate camera around the transform; otherwise rotate the object
    }

    [Header("Targets")]
    public List<TransformStatus> targets = new List<TransformStatus>();

    [Header("Camera Control")]
    public float rotationSpeed = 5f;
    public float zoomSpeed = 5f;
    public float minDistance = 5f;
    public float maxDistance = 50f;
    public bool canRotate = true;
    public bool invertPedestalDirection = false;
    public bool invertPanDirection = false;
    public float minYPosition = -10f;

    [Header("References")]
    public HomeScreen homeScreen;
    public CorneaCameraDirector Cornea;
    private CCD_Lerp cameraLerpScript;

    private int oldPosition;
    private bool previousStatus;

    private float currentDistance;
    private Vector3 lastMousePosition;
    private Vector2 lastTouchPosition;
    private float initialVerticalDistance;
    private bool touchStartedOverUI;
    private bool mouseStartedOverUI;

    void Start()
    {
        if (homeScreen == null)
            homeScreen = GameObject.FindWithTag("UIDocument")?.GetComponent<HomeScreen>();

        cameraLerpScript = GetComponent<CCD_Lerp>();

        if (targets.Count > 0 && targets[0].targetTransform != null)
        {
            initialVerticalDistance = transform.position.y - targets[0].targetTransform.position.y;
        }
    }
    void Update()
    {
        if (Cornea != null)
            oldPosition = Cornea.Lerp.GetCurrentIndex;

        if (!canRotate || cameraLerpScript.IsActive)
        {
            if (!canRotate && previousStatus && !cameraLerpScript.IsActive)
            {
                goToPosition(oldPosition);
                previousStatus = false;
            }
            return;
        }

        previousStatus = true;

        bool isMobileMode = DeviceDetection.IsMobileActive;

        if (isMobileMode)
        {
            // Real device touch input
            if (Input.touchCount > 0)
            {
                HandleTouchInput();
            }

#if UNITY_EDITOR
            // In editor, when testing mobile layout with mouse,
            // do NOT let mouse control the camera.
            return;
#endif

            return;
        }

        // Desktop mode
        HandleMouseInput();
    }
    private void HandleRotation()
    {
        if (targets.Count == 0 || targets[0].targetTransform == null) return;

        if (Input.GetMouseButtonDown(0))
            lastMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            if (Mathf.Abs(delta.x) > 0.01f)
            {
                float rotationAmount = delta.x * rotationSpeed * Time.deltaTime;

                foreach (var t in targets)
                {
                    if (t.targetTransform == null) continue;

                    if (t.rotateCamera)
                        transform.RotateAround(t.targetTransform.position, Vector3.up, rotationAmount);
                    else
                        t.targetTransform.Rotate(Vector3.up, rotationAmount);
                }

                homeScreen?.NotifyUserRotated();
            }

            lastMousePosition = Input.mousePosition;
        }
    }
    private void HandleZoom()
    {
        if (targets.Count == 0 || targets[0].targetTransform == null) return;

        // Block zoom if pointer is over UI
        if (UIBlockerRaycast.Instance != null && UIBlockerRaycast.Instance.IsPointerOverUI())
            return;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        // If no scroll input, skip
        if (Mathf.Abs(scrollInput) < 0.0001f)
            return;

        currentDistance = Vector3.Distance(transform.position, targets[0].targetTransform.position);
        currentDistance = Mathf.Clamp(currentDistance - scrollInput * zoomSpeed, minDistance, maxDistance);

        Vector3 direction = (transform.position - targets[0].targetTransform.position).normalized;
        transform.position = targets[0].targetTransform.position + direction * currentDistance;
    }
    private void HandleMouseInput()
    {
        bool overUI = UIBlockerRaycast.Instance != null && UIBlockerRaycast.Instance.IsPointerOverUI();

        if (Input.GetMouseButtonDown(0))
        {
            mouseStartedOverUI = overUI;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
            mouseStartedOverUI = false;

        // Prevent wheel zoom while cursor is over UI
        if (!overUI)
            HandleZoom();

        // Prevent rotation only if drag started on UI
        if (!mouseStartedOverUI)
            HandleRotation();
    }
    private void HandleTouchInput()
    {
        if (targets.Count == 0 || targets[0].targetTransform == null) return;
        if (Input.touchCount < 2) return;

        if (IsAnyTouchOverUI())
            return;

        if (HandleDualTouchZoom())
            return;

        HandleDualTouchRotate();
    }

    private bool IsAnyTouchOverUI()
    {
        if (UIBlockerRaycast.Instance == null) return false;
        if (Input.touchCount < 2) return false;

        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        return UIBlockerRaycast.Instance.IsScreenPositionOverUI(touch0.position) ||
               UIBlockerRaycast.Instance.IsScreenPositionOverUI(touch1.position);
    }

    private bool HandleDualTouchZoom()
    {
        if (Input.touchCount < 2) return false;

        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
        Vector2 prevPos1 = touch1.position - touch1.deltaPosition;

        float previousDistance = (prevPos0 - prevPos1).magnitude;
        float currentDistanceBetweenTouches = (touch0.position - touch1.position).magnitude;

        float pinchDelta = currentDistanceBetweenTouches - previousDistance;

        if (Mathf.Abs(pinchDelta) < 0.5f)
            return false;

        currentDistance = Vector3.Distance(transform.position, targets[0].targetTransform.position);
        currentDistance = Mathf.Clamp(currentDistance - pinchDelta * zoomSpeed * 0.01f, minDistance, maxDistance);

        Vector3 direction = (transform.position - targets[0].targetTransform.position).normalized;
        transform.position = targets[0].targetTransform.position + direction * currentDistance;

        return true;
    }

    private void HandleDualTouchRotate()
    {
        if (Input.touchCount < 2) return;

        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 averageDelta = (touch0.deltaPosition + touch1.deltaPosition) * 0.5f;

        if (Mathf.Abs(averageDelta.x) <= Mathf.Abs(averageDelta.y))
            return;

        float rotationAmount = averageDelta.x * rotationSpeed * Time.deltaTime;

        foreach (var t in targets)
        {
            if (t.targetTransform == null) continue;

            if (t.rotateCamera)
                transform.RotateAround(t.targetTransform.position, Vector3.up, rotationAmount);
            else
                t.targetTransform.Rotate(Vector3.up, rotationAmount);
        }

        homeScreen?.NotifyUserRotated();
    }
    private void HandlePinchZoom()
    {
        if (targets.Count == 0 || targets[0].targetTransform == null) return;
        if (Input.touchCount < 2) return;
        if (touchStartedOverUI) return;

        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

        float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
        float currentMagnitude = (touch0.position - touch1.position).magnitude;

        float difference = currentMagnitude - prevMagnitude;
        if (Mathf.Abs(difference) < 0.01f) return;

        currentDistance = Vector3.Distance(transform.position, targets[0].targetTransform.position);
        currentDistance = Mathf.Clamp(currentDistance - difference * zoomSpeed * 0.01f, minDistance, maxDistance);

        Vector3 direction = (transform.position - targets[0].targetTransform.position).normalized;
        transform.position = targets[0].targetTransform.position + direction * currentDistance;
    }
    private void HandleTouchPan()
    {
        if (Input.touchCount < 2) return;

        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        if (touch0.phase == TouchPhase.Moved && touch1.phase == TouchPhase.Moved)
        {
            Vector2 delta0 = touch0.deltaPosition;
            Vector2 delta1 = touch1.deltaPosition;

            if (Vector2.Dot(delta0.normalized, delta1.normalized) > 0.9f)
            {
                Vector3 move = new Vector3(
                    (invertPanDirection ? delta0.x : -delta0.x) * 0.5f * Time.deltaTime,
                    (invertPanDirection ? delta0.y : -delta0.y) * 0.5f * Time.deltaTime,
                    0
                );
                transform.Translate(move, Space.Self);

                Vector3 clamped = transform.position;
                clamped.y = Mathf.Max(clamped.y, minYPosition);
                transform.position = clamped;
            }
        }
    }
    public void goToPosition(int pos)
    {
        Cornea.Lerp.CameraLerp(pos);
    }
    public void SetTarget(Transform newTarget, bool rotateCamera)
    {
        if (targets.Count == 0)
            targets.Add(new TransformStatus { targetTransform = newTarget, rotateCamera = rotateCamera });
        else
            targets[0] = new TransformStatus { targetTransform = newTarget, rotateCamera = rotateCamera };
    }
}