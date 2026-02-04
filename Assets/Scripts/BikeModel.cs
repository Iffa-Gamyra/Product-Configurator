using UnityEngine;

[CreateAssetMenu(fileName = "BikeModel", menuName = "Bike/BikeModel")]
public class BikeModel : ScriptableObject
{
    public string modelName;          // e.g., "Model A", "Model B"
    public GameObject model;           //model 
    public int basePrice;             // Base bike price

    [Header("Body Options")]
    public ColorOption[] bodyColors;

    [Header("Seat Options")]
    public ColorOption[] seatColors;

    [Header("Badge Options")]
    public ColorOption[] badgeColors;
}

[System.Serializable]
public class ColorOption
{
    public string displayName;
    public Material material;
    public int price;
}
