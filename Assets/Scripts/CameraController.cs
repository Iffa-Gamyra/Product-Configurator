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
    public EnableLocation cameraPosition;
    private CCD_Lerp cameraLerpScript;

    private int oldPosition;
    private bool previousStatus;

    private float currentDistance;
    private Vector3 lastMousePosition;
    private Vector2 lastTouchPosition;
    private float initialVerticalDistance;

    void Start()
    {
        homeScreen = GameObject.FindWithTag("UIDocument")?.GetComponent<HomeScreen>();
        cameraLerpScript = GetComponent<CCD_Lerp>();

        if (targets.Count > 0 && targets[0].targetTransform != null)
        {
            initialVerticalDistance = transform.position.y - targets[0].targetTransform.position.y;
        }
    }

    void Update()
    {
        if (cameraPosition != null)
            oldPosition = cameraPosition.Cornea.Lerp.GetCurrentIndex;

        if (canRotate && !cameraLerpScript.IsActive)
        {
            previousStatus = true;
            HandleTouchInput();
            HandleRotation();
            HandleZoom();
        }
        else if (!canRotate && previousStatus && !cameraLerpScript.IsActive)
        {
            cameraPosition?.goToPosition(oldPosition);
            previousStatus = false;
        }
    }

    private void HandleRotation()
    {
        if (targets.Count == 0 || targets[0].targetTransform == null) return;

        if (Input.GetMouseButtonDown(0))
            lastMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float rotationAmount = delta.x * rotationSpeed * Time.deltaTime;

            foreach (var t in targets)
            {
                if (t.targetTransform == null) continue;

                if (t.rotateCamera)
                    transform.RotateAround(t.targetTransform.position, Vector3.up, rotationAmount);
                else
                    t.targetTransform.Rotate(Vector3.up, rotationAmount);
            }

            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleZoom()
    {
        if (targets.Count == 0 || targets[0].targetTransform == null) return;

        currentDistance = Vector3.Distance(transform.position, targets[0].targetTransform.position);
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        currentDistance = Mathf.Clamp(currentDistance - scrollInput * zoomSpeed, minDistance, maxDistance);

        Vector3 direction = (transform.position - targets[0].targetTransform.position).normalized;
        transform.position = targets[0].targetTransform.position + direction * currentDistance;
    }

    private void HandleTouchInput()
    {
        if (targets.Count == 0 || targets[0].targetTransform == null) return;

        if (Input.touchCount == 2)
            HandleTouchPan();
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                lastTouchPosition = touch.position;
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.position - lastTouchPosition;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    float rotationAmount = delta.x * rotationSpeed * Time.deltaTime;
                    transform.RotateAround(targets[0].targetTransform.position, Vector3.up, rotationAmount);
                }
                else
                {
                    float zoomAmount = delta.y * zoomSpeed * Time.deltaTime;
                    currentDistance = Vector3.Distance(transform.position, targets[0].targetTransform.position);
                    currentDistance = Mathf.Clamp(currentDistance - zoomAmount, minDistance, maxDistance);

                    Vector3 direction = (transform.position - targets[0].targetTransform.position).normalized;
                    transform.position = targets[0].targetTransform.position + direction * currentDistance;
                }
                lastTouchPosition = touch.position;
            }
        }
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

    public void SetTarget(Transform newTarget, bool rotateCamera)
    {
        if (targets.Count == 0)
            targets.Add(new TransformStatus { targetTransform = newTarget, rotateCamera = rotateCamera });
        else
            targets[0] = new TransformStatus { targetTransform = newTarget, rotateCamera = rotateCamera };
    }
}
