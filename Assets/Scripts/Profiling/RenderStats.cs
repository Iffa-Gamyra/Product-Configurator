using System.Text;
using Unity.Profiling;
using UnityEngine;
using TMPro; // Import the TextMeshPro namespace

public class RenderStats : MonoBehaviour
{
    public TextMeshProUGUI statsTextUI; // Reference to the TextMeshProUGUI component
    string statsText;
    ProfilerRecorder setPassCallsRecorder;
    ProfilerRecorder drawCallsRecorder;
    ProfilerRecorder verticesRecorder;

    void OnEnable()
    {
        setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
    }

    void OnDisable()
    {
        setPassCallsRecorder.Dispose();
        drawCallsRecorder.Dispose();
        verticesRecorder.Dispose();
    }

    void Update()
    {
        var sb = new StringBuilder(500);
        if (setPassCallsRecorder.Valid)
            sb.AppendLine($"SetPass Calls: {setPassCallsRecorder.LastValue}");
        if (drawCallsRecorder.Valid)
            sb.AppendLine($"Draw Calls: {drawCallsRecorder.LastValue}");
        if (verticesRecorder.Valid)
            sb.AppendLine($"Vertices: {verticesRecorder.LastValue}");
        statsText = sb.ToString();

        // Update the TextMeshPro text directly
        if (statsTextUI != null)
            statsTextUI.text = statsText;
    }
}