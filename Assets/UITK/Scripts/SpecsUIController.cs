using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpecsUIController
{
    private readonly VisualTreeAsset specRowTextTemplate;
    private readonly VisualTreeAsset specRowBarTemplate;
    private readonly VisualTreeAsset specRowToggleTemplate;
    private readonly VisualTreeAsset specRowChipsTemplate;

    private static readonly List<string> chipBuffer = new();

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

    public void PopulateSpecs(VisualElement container, Product product)
    {
        if (container == null) return;
        container.Clear();
        if (product?.specs == null || product.specs.Length == 0) return;

        foreach (var spec in product.specs)
        {
            VisualElement row = null;

            switch (spec.type)
            {
                case Product.SpecType.Text:
                    row = specRowTextTemplate?.Instantiate();
                    if (row == null) break;
                    row.Q<Label>(UINames.SpecRow_TextLabel).text = spec.label;
                    row.Q<Label>(UINames.SpecRow_TextValue).text = spec.value;
                    break;

                case Product.SpecType.Bar:
                case Product.SpecType.InvertedBar:
                    row = specRowBarTemplate?.Instantiate();
                    if (row == null) break;
                    row.Q<Label>(UINames.SpecRow_BarLabel).text = spec.label;
                    var valLabel = row.Q<Label>(UINames.SpecRow_BarValue);
                    if (valLabel != null) valLabel.text = spec.value;
                    var bar = row.Q<ProgressBar>(UINames.SpecRow_BarProgress);
                    if (bar != null)
                    {
                        bar.lowValue = 0f;
                        bar.highValue = Mathf.Max(0.0001f, spec.max);
                        float clamped = Mathf.Clamp(spec.current, 0f, bar.highValue);
                        bar.value = spec.type == Product.SpecType.InvertedBar
                            ? bar.highValue - clamped : clamped;
                    }
                    break;

                case Product.SpecType.Toggle:
                    row = specRowToggleTemplate?.Instantiate();
                    if (row == null) break;
                    row.Q<Label>(UINames.SpecRow_ToggleLabel).text = spec.label;
                    var toggle = row.Q<Toggle>(UINames.SpecRow_ToggleValue);
                    if (toggle != null)
                    {
                        toggle.SetEnabled(false);
                        toggle.value = spec.toggle == Product.ToggleState.Yes ||
                                       spec.toggle == Product.ToggleState.Optional;
                        toggle.text = spec.toggle.ToString();
                    }
                    break;

                case Product.SpecType.Chips:
                    row = specRowChipsTemplate?.Instantiate();
                    if (row == null) break;
                    row.Q<Label>(UINames.SpecRow_ChipLabel).text = spec.label;
                    var chips = row.Q<VisualElement>(UINames.SpecRow_ChipsContainer);
                    if (chips != null)
                    {
                        chips.Clear();
                        var parts = SplitChips(spec.value);
                        for (int j = 0; j < parts.Count; j++)
                        {
                            var c = new Label(parts[j]);
                            c.AddToClassList("chip");
                            chips.Add(c);
                        }
                    }
                    break;
            }

            if (row != null) container.Add(row);
        }
    }

    private static List<string> SplitChips(string s)
    {
        chipBuffer.Clear();
        if (string.IsNullOrEmpty(s)) return chipBuffer;
        int start = 0;
        for (int i = 0; i <= s.Length; i++)
        {
            if (i == s.Length || s[i] == '|')
            {
                int len = i - start;
                if (len > 0)
                {
                    string part = s.Substring(start, len).Trim();
                    if (part.Length > 0) chipBuffer.Add(part);
                }
                start = i + 1;
            }
        }
        return chipBuffer;
    }
}