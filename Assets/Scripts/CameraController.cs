using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [System.Serializable]
    public struct TransformStatus
    {
        public Transform targetTransform;
        public bool rotateCamera;
    }

    [Header("Targets")]
    public TransformStatus target = new TransformStatus();

    [Header("Camera Control")]
    public float rotationSpeed = 5f;
    public float zoomSpeed = 5f;
    public float minDistance = 5f;
    public float maxDistance = 50f;
    public bool canRotate = true;

    [Header("References")]
    public HomeScreen desktopHomeScreen;
    public HomeScreen mobileHomeScreen;
    public CorneaCameraDirector Cornea;
    private CCD_Lerp cameraLerpScript;

    private int oldPosition;
    private bool previousStatus;

    private float currentDistance;
    private Vector3 lastMousePosition;
    private Vector2 lastTouchPosition;

    private bool mouseStartedOverUI;
    private bool touchStartedOverUI;

    void Start()
    {
        cameraLerpScript = GetComponent<CCD_Lerp>();
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
            HandleTouchInput();
            return;
        }

        HandleMouseInput();
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

        if (!overUI)
            HandleMouseZoom();

        if (!mouseStartedOverUI)
            HandleMouseRotation();
    }

    private void HandleMouseRotation()
    {
        if (target.targetTransform == null) return;

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            if (Mathf.Abs(delta.x) > 0.01f)
            {
                float rotationAmount = delta.x * rotationSpeed * Time.deltaTime;

                if (target.rotateCamera)
                    transform.RotateAround(target.targetTransform.position, Vector3.up, rotationAmount);
                else
                    target.targetTransform.Rotate(Vector3.up, rotationAmount);

                desktopHomeScreen?.NotifyUserRotated();
            }

            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleMouseZoom()
    {
        if (target.targetTransform == null) return;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) < 0.0001f)
            return;

        currentDistance = Vector3.Distance(transform.position, target.targetTransform.position);
        currentDistance = Mathf.Clamp(currentDistance - scrollInput * zoomSpeed, minDistance, maxDistance);

        Vector3 direction = (transform.position - target.targetTransform.position).normalized;
        transform.position = target.targetTransform.position + direction * currentDistance;
    }

    private void HandleTouchInput()
    {
        if (target.targetTransform == null) return;

        if (Input.touchCount == 1)
        {
            HandleTouchRotation();
            return;
        }

        if (Input.touchCount >= 2)
        {
            if (IsAnyTouchOverUI())
                return;

            HandlePinchZoom();
        }
    }

    private void HandleTouchRotation()
    {
        Touch touch = Input.GetTouch(0);

        bool overUI = UIBlockerRaycast.Instance != null &&
                      UIBlockerRaycast.Instance.IsScreenPositionOverUI(touch.position);

        if (touch.phase == TouchPhase.Began)
        {
            touchStartedOverUI = overUI;
            lastTouchPosition = touch.position;
            return;
        }

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            touchStartedOverUI = false;
            return;
        }

        if (touchStartedOverUI)
            return;

        if (touch.phase == TouchPhase.Moved)
        {
            Vector2 delta = touch.position - lastTouchPosition;

            // horizontal-only rotation
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y) && Mathf.Abs(delta.x) > 0.01f)
            {
                float rotationAmount = delta.x * rotationSpeed * Time.deltaTime;

                if (target.rotateCamera)
                    transform.RotateAround(target.targetTransform.position, Vector3.up, rotationAmount);
                else
                    target.targetTransform.Rotate(Vector3.up, rotationAmount);

                mobileHomeScreen?.NotifyUserRotated();
            }

            lastTouchPosition = touch.position;
        }
    }

    private void HandlePinchZoom()
    {
        if (Input.touchCount < 2) return;

        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
        Vector2 prevPos1 = touch1.position - touch1.deltaPosition;

        float previousDistance = (prevPos0 - prevPos1).magnitude;
        float currentDistanceBetweenTouches = (touch0.position - touch1.position).magnitude;

        float pinchDelta = currentDistanceBetweenTouches - previousDistance;

        if (Mathf.Abs(pinchDelta) < 0.5f)
            return;

        currentDistance = Vector3.Distance(transform.position, target.targetTransform.position);
        currentDistance = Mathf.Clamp(currentDistance - pinchDelta * zoomSpeed * 0.01f, minDistance, maxDistance);

        Vector3 direction = (transform.position - target.targetTransform.position).normalized;
        transform.position = target.targetTransform.position + direction * currentDistance;
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

    public void goToPosition(int pos)
    {
        if (Cornea == null || Cornea.Lerp == null)
        {
            Debug.LogWarning($"Invalid camera move: {pos} - Cornea or Lerp is null");
            return;
        }

        Cornea.Lerp.CameraLerp(pos);
    }

    public void SetTarget(Transform newTarget, bool rotateCamera)
    {
        target = new TransformStatus
        {
            targetTransform = newTarget,
            rotateCamera = rotateCamera
        };
    }
}