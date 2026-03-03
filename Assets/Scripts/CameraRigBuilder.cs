using System.Collections.Generic;
using UnityEngine;

public sealed class CameraRigBuilder
{
    private readonly CameraController cameraController;

    private readonly Transform swoopPosition;
    private readonly Transform startPosition;
    private readonly Transform modelViewPosition;
    private readonly Transform videoPosition;

    private readonly Dictionary<string, Transform[]> rigCache = new();

    public CameraRigBuilder(
        CameraController cameraController,
        Transform swoopPosition,
        Transform startPosition,
        Transform modelViewPosition,
        Transform videoPosition)
    {
        this.cameraController = cameraController;
        this.swoopPosition = swoopPosition;
        this.startPosition = startPosition;
        this.modelViewPosition = modelViewPosition;
        this.videoPosition = videoPosition;
    }

    public void ApplyBaseRig(int firstDynamicCamIndex)
    {
        if (cameraController == null || cameraController.Cornea == null) return;

        var rig = new Transform[firstDynamicCamIndex];
        rig[0] = swoopPosition;
        rig[1] = startPosition;
        rig[2] = modelViewPosition;
        rig[3] = videoPosition;

        cameraController.Cornea.LerpCameraPositions = rig;
    }

    public void ApplyModelRig(SimulatorModel model, int firstDynamicCamIndex)
    {
        if (cameraController == null || cameraController.Cornea == null || model == null) return;

        if (!rigCache.TryGetValue(model.id, out var rig) || rig == null)
        {
            int inspectCount = model.inspectPoints != null ? model.inspectPoints.Length : 0;
            rig = new Transform[firstDynamicCamIndex + inspectCount];

            rig[0] = swoopPosition;
            rig[1] = startPosition;
            rig[2] = modelViewPosition;
            rig[3] = videoPosition;

            for (int i = 0; i < inspectCount; i++)
                rig[firstDynamicCamIndex + i] = model.inspectPoints[i]?.cameraAnchor;

            rigCache[model.id] = rig;
        }

        cameraController.Cornea.LerpCameraPositions = rig;
    }

    public void ClearCache() => rigCache.Clear();
}