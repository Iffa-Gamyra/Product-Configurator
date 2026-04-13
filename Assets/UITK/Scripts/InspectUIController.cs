using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class InspectUIController
{
    private readonly CameraController cameraController;
    private readonly int camModelViewIndex;
    private readonly int firstDynamicCamIndex;
    private readonly VisualElement inspectListContainer;
    private readonly VisualTreeAsset inspectRowTemplate;
    private readonly Texture2D iconFocus;
    private readonly Button resetViewButton;
    private readonly Button inspectPrevButton;
    private readonly Button inspectNextButton;

    private readonly List<Button> inspectButtons = new();
    private readonly List<Label> inspectLabels = new();
    private readonly List<int> inspectCameraIndices = new();

    private int activeInspectCameraIndex = -1;
    public int ActiveInspectCameraIndex => activeInspectCameraIndex;

    public InspectUIController(
        CameraController cameraController,
        int camModelViewIndex,
        int firstDynamicCamIndex,
        VisualElement inspectListContainer,
        VisualTreeAsset inspectRowTemplate,
        Button resetViewButton,
        Button inspectPrevButton,
        Button inspectNextButton,
        Texture2D iconFocus = null)
    {
        this.cameraController = cameraController;
        this.camModelViewIndex = camModelViewIndex;
        this.firstDynamicCamIndex = firstDynamicCamIndex;
        this.inspectListContainer = inspectListContainer;
        this.inspectRowTemplate = inspectRowTemplate;
        this.resetViewButton = resetViewButton;
        this.inspectPrevButton = inspectPrevButton;
        this.inspectNextButton = inspectNextButton;
        this.iconFocus = iconFocus;

        if (this.resetViewButton != null)
            this.resetViewButton.style.display = DisplayStyle.None;
    }

    public void Rebuild(Product product)
    {
        if (inspectListContainer == null || inspectRowTemplate == null) return;

        inspectListContainer.Clear();
        inspectButtons.Clear();
        inspectLabels.Clear();
        inspectCameraIndices.Clear();
        activeInspectCameraIndex = -1;

        if (product?.inspectPoints == null || product.inspectPoints.Length == 0)
        {
            activeInspectCameraIndex = -1;
            UpdateSelectionUI();
            return;
        }

        int validIndex = 0;

        for (int i = 0; i < product.inspectPoints.Length; i++)
        {
            var point = product.inspectPoints[i];
            if (point == null || point.cameraAnchor == null)
                continue;

            int cameraIndex = firstDynamicCamIndex + validIndex;
            validIndex++;
        
            var row = inspectRowTemplate.Instantiate();
            var btn = row.Q<Button>(UINames.InspectRow_Btn);
            var label = row.Q<Label>(UINames.InspectRow_Label);

            if (label != null)
                label.text = point.label;

            if (btn != null)
            {
                inspectButtons.Add(btn);
                inspectLabels.Add(label);
                inspectCameraIndices.Add(cameraIndex);

                int captured = cameraIndex;
                btn.clicked += () => OnInspectClicked(captured);
            }

            inspectListContainer.Add(row);
        }

        UpdateSelectionUI();
    }

    public void ResetView()
    {
        if (inspectCameraIndices == null || inspectCameraIndices.Count == 0)
            return;

        activeInspectCameraIndex = -1;
        cameraController?.goToPosition(camModelViewIndex);
        UpdateSelectionUI();
    }

    public void InspectNext()
    {
        int count = inspectCameraIndices.Count;
        if (count == 0) return;
        int idx = GetActiveIndex();
        int nextIdx = idx < 0 ? 0 : Mathf.Min(idx + 1, count - 1);
        GoTo(inspectCameraIndices[nextIdx]);
    }

    public void InspectPrev()
    {
        int count = inspectCameraIndices.Count;
        if (count == 0) return;
        int idx = GetActiveIndex();
        int prevIdx = idx < 0 ? 0 : Mathf.Max(idx - 1, 0);
        GoTo(inspectCameraIndices[prevIdx]);
    }

    private void GoTo(int cameraIndex)
    {
        activeInspectCameraIndex = cameraIndex;
        cameraController?.goToPosition(cameraIndex);
        UpdateSelectionUI();
    }

    private void OnInspectClicked(int cameraIndex)
    {
        activeInspectCameraIndex =
            activeInspectCameraIndex == cameraIndex ? -1 : cameraIndex;

        if (activeInspectCameraIndex < 0)
            cameraController?.goToPosition(camModelViewIndex);
        else
            cameraController?.goToPosition(cameraIndex);

        UpdateSelectionUI();
    }

    private int GetActiveIndex()
    {
        for (int i = 0; i < inspectCameraIndices.Count; i++)
            if (inspectCameraIndices[i] == activeInspectCameraIndex) return i;
        return -1;
    }

    private void UpdateSelectionUI()
    {
        int idx = GetActiveIndex();
        bool active = idx >= 0;

        for (int i = 0; i < inspectButtons.Count; i++)
        {
            var btn = inspectButtons[i];
            if (btn == null) continue;
            bool isActive = inspectCameraIndices[i] == activeInspectCameraIndex;
            btn.EnableInClassList("active", isActive);
            if (i < inspectLabels.Count && inspectLabels[i] != null)
                inspectLabels[i].EnableInClassList("active", isActive);
        }

        if (resetViewButton != null)
            resetViewButton.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;

        bool canPrev = active && idx > 0;
        bool canNext = active && idx < inspectCameraIndices.Count - 1;

        if (inspectPrevButton?.parent != null)
            inspectPrevButton.parent.style.display = canPrev ? DisplayStyle.Flex : DisplayStyle.None;
        if (inspectNextButton?.parent != null)
            inspectNextButton.parent.style.display = canNext ? DisplayStyle.Flex : DisplayStyle.None;
    }
}