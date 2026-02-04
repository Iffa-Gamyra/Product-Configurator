using UnityEngine;
using UnityEngine.UIElements;

public enum SectionType { Home, Video, Bike, Merch }

[CreateAssetMenu(fileName = "UISection", menuName = "UI/UISection")]
public class UISection : ScriptableObject
{
    public string sectionName;          // Unique name for this UI section
    public VisualTreeAsset uxmlAsset;   // UXML asset for this section
    public int cameraPositionIndex;     // Camera position index from EnableLocation
    public SectionType sectionType;     // Type of this section
}
