using System.Text;
using Unity.Profiling;
using UnityEngine;
using TMPro; // Import the TextMeshPro namespace

public class MemoryStatsDisplay : MonoBehaviour
{
    public TextMeshProUGUI memoryTextUI; // Reference to the TextMeshProUGUI component
    ProfilerRecorder totalAllocatedMemoryRecorder;
    ProfilerRecorder totalReservedMemoryRecorder;
    ProfilerRecorder totalUnusedReservedMemoryRecorder;

    void OnEnable()
    {
        // Start recording memory stats
        totalAllocatedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
        totalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
        totalUnusedReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Unused Reserved Memory");
    }

    void OnDisable()
    {
        // Dispose recorders to free resources
        totalAllocatedMemoryRecorder.Dispose();
        totalReservedMemoryRecorder.Dispose();
        totalUnusedReservedMemoryRecorder.Dispose();
    }

    void Update()
    {
        var sb = new StringBuilder(500);
        if (totalAllocatedMemoryRecorder.Valid)
            sb.AppendLine($"Total Used Memory: {BytesToMB(totalAllocatedMemoryRecorder.LastValue)} MB");
        if (totalReservedMemoryRecorder.Valid)
            sb.AppendLine($"Total Reserved Memory: {BytesToMB(totalReservedMemoryRecorder.LastValue)} MB");
        if (totalUnusedReservedMemoryRecorder.Valid)
            sb.AppendLine($"Total Unused Reserved Memory: {BytesToMB(totalUnusedReservedMemoryRecorder.LastValue)} MB");

        if (memoryTextUI != null)
            memoryTextUI.text = sb.ToString();
    }

    // Helper method to convert bytes to megabytes
    private float BytesToMB(long bytes)
    {
        return bytes / (1024f * 1024f);
    }
}
