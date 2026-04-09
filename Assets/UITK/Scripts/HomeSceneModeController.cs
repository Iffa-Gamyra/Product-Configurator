using System;
using UnityEngine;

public class HomeSceneModeController
{
    private readonly Camera mainCam;
    private readonly CameraController cameraController;
    private readonly DecalController decalController;
    private readonly Transform rotationTarget;
    private readonly ScreenNavigator nav;
    private readonly Func<bool> isAnyProductScreenVisible;
    private readonly float videoFov;
    private readonly float normalFov;

    public HomeSceneModeController(
        Camera mainCam,
        CameraController cameraController,
        DecalController decalController,
        Transform rotationTarget,
        ScreenNavigator nav,
        Func<bool> isAnyProductScreenVisible,
        float videoFov,
        float normalFov)
    {
        this.mainCam = mainCam;
        this.cameraController = cameraController;
        this.decalController = decalController;
        this.rotationTarget = rotationTarget;
        this.nav = nav;
        this.isAnyProductScreenVisible = isAnyProductScreenVisible;
        this.videoFov = videoFov;
        this.normalFov = normalFov;
    }

    public void SetVideoFov(bool isVideo)
    {
        if (mainCam == null) return;
        mainCam.fieldOfView = isVideo ? videoFov : normalFov;
    }

    public void SetProductDecalVisible(bool showProductDecal)
    {
        if (decalController == null) return;

        if (showProductDecal && !decalController.IsOpaque)
            decalController.StartFadeInAndScaleUp();
        else if (!showProductDecal && decalController.IsOpaque)
            decalController.StartFadeOutAndScaleDown();
    }

    public void RefreshRotationState()
    {
        bool allowRotation = isAnyProductScreenVisible() && (nav != null && !nav.IsOverlayOpen());

        if (cameraController != null)
            cameraController.canRotate = allowRotation;

        if (rotationTarget != null)
            rotationTarget.gameObject.SetActive(allowRotation);
    }
}