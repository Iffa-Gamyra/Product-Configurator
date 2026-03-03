using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpecsUIController
{
    private readonly VisualTreeAsset specRowTextTemplate;
    private readonly VisualTreeAsset specRowBarTemplate;
    private readonly VisualTreeAsset specRowToggleTemplate;
    private readonly VisualTreeAsset specRowChipsTemplate;

    public SpecsUIController(
        VisualTreeAsset text,
        VisualTreeAsset bar,
        VisualTreeAsset toggle,
        VisualTreeAsset chips)
    {
        specRowTextTemplate = text;
        specRowBarTemplate = bar;
        specRowToggleTemplate = toggle;
        specRowChipsTemplate = chips;
    }

    public void PopulateSpecs(VisualElement container, SimulatorModel model)
    {
        if (container == null) return;

        container.Clear();
        if (model == null || model.specs == null) return;

        foreach (var spec in model.specs)
        {
            VisualElement row = null;

            switch (spec.type)
            {
                case SimulatorModel.SpecType.Text:
                    row = specRowTextTemplate?.Instantiate();
                    if (row == null) break;
                    row.Q<Label>("specLabel").text = spec.label;
                    row.Q<Label>("specValue").text = spec.value;
                    break;

                case SimulatorModel.SpecType.Bar:
                case SimulatorModel.SpecType.InvertedBar:
                    row = specRowBarTemplate?.Instantiate();
                    if (row == null) break;

                    row.Q<Label>("specLabel").text = spec.label;
                    var valueLabel = row.Q<Label>("specValue");
                    if (valueLabel != null) valueLabel.text = spec.value;

                    var bar = row.Q<ProgressBar>("specBar");
                    if (bar != null)
                    {
                        bar.lowValue = 0f;
                        bar.highValue = Mathf.Max(0.0001f, spec.max);

                        float clamped = Mathf.Clamp(spec.current, 0f, bar.highValue);
                        bar.value = (spec.type == SimulatorModel.SpecType.InvertedBar)
                            ? (bar.highValue - clamped)
                            : clamped;
                    }
                    break;

                case SimulatorModel.SpecType.Toggle:
                    row = specRowToggleTemplate?.Instantiate();
                    if (row == null) break;

                    row.Q<Label>("specLabel").text = spec.label;
                    var toggle = row.Q<Toggle>("specToggle");
                    if (toggle != null)
                    {
                        toggle.SetEnabled(false);
                        toggle.value = (spec.toggle == SimulatorModel.ToggleState.Yes ||
                                        spec.toggle == SimulatorModel.ToggleState.Optional);
                        toggle.text = spec.toggle.ToString();
                    }
                    break;

                case SimulatorModel.SpecType.Chips:
                    row = specRowChipsTemplate?.Instantiate();
                    if (row == null) break;

                    row.Q<Label>("specLabel").text = spec.label;
                    var chipsContainer = row.Q<VisualElement>("chipContainer");
                    if (chipsContainer != null)
                    {
                        chipsContainer.Clear();
                        foreach (var chipText in SplitChipsNoLinq(spec.value))
                        {
                            var chip = new Label(chipText);
                            chip.AddToClassList("chip");
                            chipsContainer.Add(chip);
                        }
                    }
                    break;
            }

            if (row != null)
                container.Add(row);
        }
    }

    private static IEnumerable<string> SplitChipsNoLinq(string s)
    {
        if (string.IsNullOrEmpty(s)) yield break;

        int start = 0;
        for (int i = 0; i <= s.Length; i++)
        {
            if (i == s.Length || s[i] == '|')
            {
                int len = i - start;
                if (len > 0)
                {
                    string part = s.Substring(start, len).Trim();
                    if (part.Length > 0) yield return part;
                }
                start = i + 1;
            }
        }
    }
}