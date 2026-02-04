using UnityEngine;

[CreateAssetMenu(fileName = "ShirtOption", menuName = "Merch/ShirtOption")]
public class ShirtOption : ScriptableObject
{
    public string colorName;
    public Material material;
    public int price = 50;
}
