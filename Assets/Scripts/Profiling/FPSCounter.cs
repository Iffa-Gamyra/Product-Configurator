using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;  // Reference to the TextMeshProUGUI component
    private float deltaTime = 0.0f;

    void Update()
    {
        // Calculate smooth delta time
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        
        // Update the text display
        fpsText.text = string.Format("FPS {0:0.}", fps);
    }
}
