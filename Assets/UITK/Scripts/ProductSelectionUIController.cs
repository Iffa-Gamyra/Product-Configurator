using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class ProductSelectionUIController
{
    private readonly VisualElement productsContainer;
    private readonly VisualTreeAsset productButtonTemplate;
    private readonly List<Label> selectedProductLabels;
    private readonly Dictionary<string, Button> productButtons = new();

    public ProductSelectionUIController(
        VisualElement productsContainer,
        VisualTreeAsset productButtonTemplate,
        params Label[] selectedProductLabels)
    {
        this.productsContainer = productsContainer;
        this.productButtonTemplate = productButtonTemplate;
        this.selectedProductLabels = new List<Label>(selectedProductLabels);
    }

    public void BuildIfNeeded(IReadOnlyList<Product> products, Action<string> onSelectProductId)
    {
        if (productsContainer == null || productButtonTemplate == null) return;
        if (productsContainer.childCount > 0 && productButtons.Count > 0) return;

        productButtons.Clear();
        productsContainer.Clear();
        if (products == null) return;

        for (int i = 0; i < products.Count; i++)
        {
            var m = products[i];
            if (m == null || string.IsNullOrWhiteSpace(m.productId)) continue;

            var instance = productButtonTemplate.Instantiate();
            var btn = instance.Q<Button>(UINames.ProductBtn_Select);
            if (btn == null)
            {
                Debug.LogError($"Button '{UINames.ProductBtn_Select}' not found in productButtonTemplate.");
                continue;
            }

            btn.name = m.productId;
            btn.text = m.productName;

            string mid = m.productId;
            btn.clicked += () => onSelectProductId?.Invoke(mid);

            productsContainer.Add(instance);
            productButtons[m.productId] = btn;
        }
    }

    public void UpdateSelected(string selectedProductId, Product currentProduct)
    {
        foreach (var kv in productButtons)
            kv.Value.EnableInClassList("active", kv.Key == selectedProductId);

        string name = currentProduct != null ? currentProduct.productName : "";
        foreach (var label in selectedProductLabels)
            if (label != null) label.text = name;
    }
}