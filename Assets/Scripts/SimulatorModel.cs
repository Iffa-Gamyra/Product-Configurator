using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SimulatorModel", menuName = "GamyraDrive/Simulator Model")]
public class SimulatorModel : ScriptableObject
{
    [Header("Model")]
    public string modelName;              // "GamyraDrive C1"
    public string id;
    public GameObject modelPrefab;

    [Header("Specs")]
    public SpecRow[] specs;

    [Header("Inspect")]
    public InspectPoint[] inspectPoints;
    public enum SpecType
    {
        Text,       // Plain label + value
        Bar,        // ProgressBar
        InvertedBar, //Inverted ProgressBar
        Toggle,     // Yes / No / Optional
        Chips       // Pills / tags
    }

    [Serializable]
    public class SpecRow
    {
        public string label;
        public SpecType type;

        [Header("Display")]
        public string value;          // always shown (e.g. "208°", "SCANeR 2025.2")

        [Header("Bar (only if type = Bar / InvertedBar only)")]
        public float current;         // e.g. 208
        public float max;             // e.g. 240

        [Header("Toggle (only if type = Toggle)")]
        public ToggleState toggle;
    }

    public enum ToggleState
    {
        No,
        Yes,
        Optional
    }

    [Serializable]
    public class InspectPoint
    {
        public string label;
        public Transform cameraAnchor;
    }
}