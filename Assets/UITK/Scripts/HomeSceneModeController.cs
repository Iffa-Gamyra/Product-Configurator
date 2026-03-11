using System;
using UnityEngine;

public class HomeSceneModeController
{
    private readonly Camera mainCam;
    private readonly CameraController cameraController;
    private readonly DecalController decalController;
    private readonly Transform rotationTarget;
    private readonly ScreenNavigator nav;
    private readonly Func<bool> isAnyModelScreenVisible;
    private readonly float videoFov;
    private readonly float normalFov;

    public HomeSceneModeController(
        Camera mainCam,
        CameraController cameraController,
        DecalController decalController,
        Transform rotationTarget,
        ScreenNavigator nav,
        Func<bool> isAnyModelScreenVisible,
        float videoFov,
        float normalFov)
    {
        this.mainCam = mainCam;
        this.cameraController = cameraController;
        this.decalController = decalController;
        this.rotationTarget = rotationTarget;
        this.nav = nav;
        this.isAnyModelScreenVisible = isAnyModelScreenVisible;
        this.videoFov = videoFov;
        this.normalFov = normalFov;
    }

    public void SetVideoFov(bool isVideo)
    {
        if (mainCam == null) return;
        mainCam.fieldOfView = isVideo ? videoFov : normalFov;
    }

    public void SetModelDecalVisible(bool showModelDecal)
    {
        if (decalController == null) return;

        if (showModelDecal && !decalController.IsOpaque)
            decalController.StartFadeInAndScaleUp();
        else if (!showModelDecal && decalController.IsOpaque)
            decalController.StartFadeOutAndScaleDown();
    }

    public void RefreshRotationState()
    {
        bool allowRotation = isAnyModelScreenVisible() && (nav != null && !nav.IsOverlayOpen());

        if (cameraController != null)
            cameraController.canRotate = allowRotation;

        if (rotationTarget != null)
            rotationTarget.gameObject.SetActive(allowRotation);
    }
}