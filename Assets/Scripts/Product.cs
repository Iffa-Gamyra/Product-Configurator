using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Product", menuName = "Product Configurator/Product")]
public class Product : ScriptableObject
{
    [Header("Product")]
    public string productName;             
    public string productId;
    public GameObject productPrefab;

    [Header("Visibility")]
    public bool showSpecs = true;
    public bool showInspect = true;

    [Header("Brochure PDF (StreamingAssets)")]
    public string brochurePdfFile; 
    public string brochureDownloadName = "Brochure.pdf"; 

    [Header("Specs")]
    public SpecRow[] specs;

    [Header("Inspect")]
    public InspectPoint[] inspectPoints;
    public enum SpecType
    {
        Text,       
        Bar,        
        InvertedBar, 
        Toggle,    
        Chips     
    }

    [Serializable]
    public class SpecRow
    {
        public string label;
        public SpecType type;

        [Header("Display")]
        public string value;         

        [Header("Bar (only if type = Bar / InvertedBar only)")]
        public float current;    
        public float max;         

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