using System.Collections.Generic;
using UnityEngine;

public class ProductManager
{
    private readonly Dictionary<string, Product> productById = new();
    private readonly Dictionary<string, GameObject> instanceCache = new();
    private readonly List<Product> loadedProducts = new();

    private readonly string resourcesPath;
    private readonly Transform spawnPoint;

    private Product currentProduct;
    private GameObject activeInstance;

    public Product CurrentProduct => currentProduct;
    public IReadOnlyList<Product> LoadedProducts => loadedProducts;

    public ProductManager(string resourcesPath, Transform spawnPoint)
    {
        this.resourcesPath = resourcesPath;
        this.spawnPoint = spawnPoint;
        LoadProducts();
    }

    private void LoadProducts()
    {
        loadedProducts.Clear();
        productById.Clear();

        var all = Resources.LoadAll<Product>(resourcesPath);
        foreach (var m in all)
        {
            if (m == null) continue;
            if (string.IsNullOrWhiteSpace(m.productId)) continue;
            if (productById.ContainsKey(m.productId)) continue;

            productById[m.productId] = m;
            loadedProducts.Add(m);
        }

        loadedProducts.Sort((a, b) => string.CompareOrdinal(a.productId, b.productId));
    }

    public bool Select(string id)
    {
        if (!productById.TryGetValue(id, out var product)) return false;
        if (product == null || product.productPrefab == null) return false;

        if (activeInstance != null)
            activeInstance.SetActive(false);

        if (!instanceCache.TryGetValue(id, out var instance) || instance == null)
        {
            instance = Object.Instantiate(product.productPrefab);

            if (spawnPoint != null)
            {
                instance.transform.SetParent(spawnPoint, false);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = product.productPrefab.transform.localRotation;
            }

            instanceCache[id] = instance;
        }

        instance.SetActive(true);
        activeInstance = instance;
        currentProduct = product;
        return true;
    }
}