using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;

    private Vector2 cursorHotSpot;
    private bool isCustomCursorActive = false;
    private bool isHoldingLeftClick = false;

    void Start()
    {
        cursorHotSpot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
        UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Start with the default cursor
    }

    void Update()
    {
        if (UIBlockerRaycast.Instance != null && UIBlockerRaycast.Instance.IsPointerOverBlocker())
        {
            if (isCustomCursorActive)
            {
                UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                isCustomCursorActive = false;
            }
            return;
        }


        // Check if the left mouse button is held down
        if (Input.GetMouseButton(0))
        {
            isHoldingLeftClick = true;
        }
        else
        {
            isHoldingLeftClick = false;
        }

        // Perform a raycast from the camera towards the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the object hit by the ray has the "cursor" tag and a BoxCollider
            if (hit.collider != null && hit.collider.CompareTag("cursor"))
            {
                if (!isCustomCursorActive || !isHoldingLeftClick)
                {
                    UnityEngine.Cursor.SetCursor(cursorTexture, cursorHotSpot, CursorMode.Auto); // Change to custom cursor
                    isCustomCursorActive = true;
                }
            }
            else if (!isHoldingLeftClick)
            {
                if (isCustomCursorActive)
                {
                    UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Revert to default cursor
                    isCustomCursorActive = false;
                }
            }
        }
        else if (!isHoldingLeftClick)
        {
            if (isCustomCursorActive)
            {
                UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Revert to default cursor
                isCustomCursorActive = false;
            }
        }
    }



}