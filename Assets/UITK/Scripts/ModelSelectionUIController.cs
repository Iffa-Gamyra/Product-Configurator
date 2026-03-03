using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class ModelSelectionUIController
{
    private readonly VisualElement modelsContainer;
    private readonly VisualTreeAsset modelButtonTemplate;

    private readonly Label selectedModelInSpecScreen;
    private readonly Label selectedModelInInspectScreen;

    private readonly Dictionary<string, Button> modelButtons = new();

    public ModelSelectionUIController(
        VisualElement modelsContainer,
        VisualTreeAsset modelButtonTemplate,
        Label selectedModelInSpecScreen,
        Label selectedModelInInspectScreen)
    {
        this.modelsContainer = modelsContainer;
        this.modelButtonTemplate = modelButtonTemplate;
        this.selectedModelInSpecScreen = selectedModelInSpecScreen;
        this.selectedModelInInspectScreen = selectedModelInInspectScreen;
    }

    public void BuildIfNeeded(IReadOnlyList<SimulatorModel> models, Action<string> onSelectModelId)
    {
        if (modelsContainer == null || modelButtonTemplate == null) return;
        if (modelsContainer.childCount > 0 && modelButtons.Count > 0) return;

        modelButtons.Clear();
        modelsContainer.Clear();

        if (models == null) return;

        for (int i = 0; i < models.Count; i++)
        {
            var m = models[i];
            if (m == null || string.IsNullOrWhiteSpace(m.id)) continue;

            var instance = modelButtonTemplate.Instantiate();
            var btn = instance.Q<Button>("modelButtonTemplate");
            if (btn == null)
            {
                Debug.LogError("Model button named 'modelButtonTemplate' not found inside modelButtonTemplate UXML.");
                continue;
            }

            btn.name = m.id;
            btn.text = m.modelName;

            string mid = m.id;
            btn.clicked += () => onSelectModelId?.Invoke(mid);

            modelsContainer.Add(instance);
            modelButtons[m.id] = btn;
        }
    }

    public void UpdateSelected(string selectedModelId, SimulatorModel currentModel)
    {
        foreach (var kv in modelButtons)
            kv.Value.EnableInClassList("active", kv.Key == selectedModelId);

        string name = currentModel != null ? currentModel.modelName : "";

        if (selectedModelInSpecScreen != null)
            selectedModelInSpecScreen.text = name;

        if (selectedModelInInspectScreen != null)
            selectedModelInInspectScreen.text = name;
    }
}