using UnityEngine;

[CreateAssetMenu(fileName = "SimulatorModel", menuName = "Simulator/Simulator")]
public class SimulatorModel : ScriptableObject
{
    public string modelName;          // e.g., "Simulator name"
    public GameObject model;           //model 
    public int basePrice;             // Base model price

    [Header("Color Options")]
    public ColorOptions[] modelColors;

    [Header("Dashboard Options")]
    public ColorOptions[] dashboardColors;

    [Header("Screen Options")]
    public NumericalOption[] screenNumber;

    [Header("DoF Options")]
    public NumericalOption[] dofOptions;

    [Header("FFB Motor")]
    public MotorOption[] motorOptions;

    [Header("Specs")]
    public Specs specs;

}

[System.Serializable]
public class ColorOptions
{
    public string displayName;
    public Material material;
    public int price;
}

[System.Serializable]
public class NumericalOption
{
    public int number;
    public int price;
}

[System.Serializable]
public class MotorOption
{
    public string ffbMotor;
    public int price;
}

[System.Serializable]
public class Specs
{
    public int power;
    public int torque;
    public int horsePower;
    public int kph;
    public int range;
    public int battery;
}