using System.Collections.Generic;
using UnityEngine;

public class ModelManager
{
    private readonly Dictionary<string, SimulatorModel> modelById = new();
    private readonly Dictionary<string, GameObject> instanceCache = new();
    private readonly List<SimulatorModel> loadedModels = new();

    private readonly string resourcesPath;
    private readonly Transform spawnPoint;

    private SimulatorModel currentModel;
    private GameObject activeInstance;

    public SimulatorModel CurrentModel => currentModel;
    public IReadOnlyList<SimulatorModel> LoadedModels => loadedModels;

    public ModelManager(string resourcesPath, Transform spawnPoint)
    {
        this.resourcesPath = resourcesPath;
        this.spawnPoint = spawnPoint;
        LoadModels();
    }

    private void LoadModels()
    {
        loadedModels.Clear();
        modelById.Clear();

        var all = Resources.LoadAll<SimulatorModel>(resourcesPath);
        foreach (var m in all)
        {
            if (m == null) continue;
            if (string.IsNullOrWhiteSpace(m.id)) continue;
            if (modelById.ContainsKey(m.id)) continue;

            modelById[m.id] = m;
            loadedModels.Add(m);
        }

        loadedModels.Sort((a, b) => string.CompareOrdinal(a.id, b.id));
    }

    public bool Select(string id)
    {
        if (!modelById.TryGetValue(id, out var model)) return false;
        if (model == null || model.modelPrefab == null) return false;

        if (activeInstance != null)
            activeInstance.SetActive(false);

        if (!instanceCache.TryGetValue(id, out var instance) || instance == null)
        {
            instance = Object.Instantiate(model.modelPrefab);

            if (spawnPoint != null)
            {
                instance.transform.SetParent(spawnPoint, false);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = model.modelPrefab.transform.localRotation;
            }

            instanceCache[id] = instance;
        }

        instance.SetActive(true);
        activeInstance = instance;
        currentModel = model;
        return true;
    }
}