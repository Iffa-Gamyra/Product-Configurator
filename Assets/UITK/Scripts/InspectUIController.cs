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

    private readonly Button resetViewButton;
    private readonly Button inspectPrevButton;
    private readonly Button inspectNextButton;

    private readonly List<Button> inspectButtons = new();
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
        Button inspectNextButton)
    {
        this.cameraController = cameraController;
        this.camModelViewIndex = camModelViewIndex;
        this.firstDynamicCamIndex = firstDynamicCamIndex;

        this.inspectListContainer = inspectListContainer;
        this.inspectRowTemplate = inspectRowTemplate;

        this.resetViewButton = resetViewButton;
        this.inspectPrevButton = inspectPrevButton;
        this.inspectNextButton = inspectNextButton;

        if (this.resetViewButton != null)
            this.resetViewButton.style.display = DisplayStyle.None;
    }

    public void Rebuild(SimulatorModel model)
    {
        if (inspectListContainer == null || inspectRowTemplate == null)
            return;

        inspectListContainer.Clear();
        inspectButtons.Clear();
        inspectCameraIndices.Clear();
        activeInspectCameraIndex = -1;

        if (model == null || model.inspectPoints == null)
        {
            UpdateSelectionUI();
            return;
        }

        for (int i = 0; i < model.inspectPoints.Length; i++)
        {
            int cameraIndex = firstDynamicCamIndex + i;
            var point = model.inspectPoints[i];
            if (point == null) continue;

            var row = inspectRowTemplate.Instantiate();
            var btn = row.Q<Button>("inspectButton");
            var label = row.Q<Label>("inspectLabel");

            if (label != null)
                label.text = point.label;

            if (btn != null)
            {
                inspectButtons.Add(btn);
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
        activeInspectCameraIndex = -1;
        cameraController?.goToPosition(camModelViewIndex);
        UpdateSelectionUI();
    }

    public void InspectNext()
    {
        int count = inspectCameraIndices.Count;
        if (count == 0) return;

        int idx = GetActiveInspectListIndex();
        int nextIdx = (idx < 0) ? 0 : Mathf.Min(idx + 1, count - 1);

        GoToInspectIndex(inspectCameraIndices[nextIdx]);
    }

    public void InspectPrev()
    {
        int count = inspectCameraIndices.Count;
        if (count == 0) return;

        int idx = GetActiveInspectListIndex();
        int prevIdx = (idx < 0) ? 0 : Mathf.Max(idx - 1, 0);

        GoToInspectIndex(inspectCameraIndices[prevIdx]);
    }

    private void GoToInspectIndex(int cameraIndex)
    {
        activeInspectCameraIndex = cameraIndex;
        cameraController?.goToPosition(cameraIndex);
        UpdateSelectionUI();
    }

    private void OnInspectClicked(int cameraIndex)
    {
        if (activeInspectCameraIndex == cameraIndex)
        {
            activeInspectCameraIndex = -1;
            cameraController?.goToPosition(camModelViewIndex);
        }
        else
        {
            activeInspectCameraIndex = cameraIndex;
            cameraController?.goToPosition(cameraIndex);
        }

        UpdateSelectionUI();
    }

    private int GetActiveInspectListIndex()
    {
        for (int i = 0; i < inspectCameraIndices.Count; i++)
            if (inspectCameraIndices[i] == activeInspectCameraIndex)
                return i;

        return -1;
    }

    private void UpdateSelectionUI()
    {
        // Highlight selection
        for (int i = 0; i < inspectButtons.Count; i++)
        {
            int cameraIndex = inspectCameraIndices[i];
            var button = inspectButtons[i];
            if (button == null) continue;

            bool isActive = (cameraIndex == activeInspectCameraIndex);
            button.EnableInClassList("active", isActive);

            var row = button.parent;
            var label = row?.Q<Label>("inspectLabel");
            if (label != null)
                label.EnableInClassList("active", isActive);
        }

        // Reset visible only when an inspect point selected
        if (resetViewButton != null)
        {
            resetViewButton.style.display =
                (GetActiveInspectListIndex() >= 0) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // Prev/next containers visibility 
        int idx2 = GetActiveInspectListIndex();
        bool inspecting = idx2 >= 0;

        bool canPrev = inspecting && idx2 > 0;
        bool canNext = inspecting && idx2 < inspectCameraIndices.Count - 1;

        if (inspectPrevButton != null)
        {
            var container = inspectPrevButton.parent;
            if (container != null)
                container.style.display = canPrev ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (inspectNextButton != null)
        {
            var container = inspectNextButton.parent;
            if (container != null)
                container.style.display = canNext ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}