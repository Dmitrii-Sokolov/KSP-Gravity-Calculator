using UnityEngine;

[CreateAssetMenu(fileName = "NewEngine", menuName = "Parts/SolidFuelEngine")]
public class SolidFuelEngine : Engine
{
    /// <summary>
    /// Fuel mass, kg
    /// </summary>
    public float FuelMass;

    /// <summary>
    /// Burn time, s
    /// </summary>
    public float BurnTime => FuelMass / FuelConsumption;
}
