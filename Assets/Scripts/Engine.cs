using UnityEngine;

[CreateAssetMenu(fileName = "NewEngine", menuName = "Parts/Engine")]
public class Engine : Part
{
    /// <summary>
    /// Thrust at vacuum, kN
    /// </summary>
    [Space]
    [Label("Thrust at vacuum, kN")]
    public float Thrust;

    public float ThrustAtOneAtmosphere => Thrust * ImpulseAtOneAtmosphere / Impulse;

    /// <summary>
    /// Fuel Consumption, kg/s
    /// </summary>
    public float FuelConsumption => 1000f * Thrust / ImpulseByMass;

    /// <summary>
    /// Specific impulse, s
    /// </summary>
    [Label("Specific impulse, s")]
    public float Impulse;

    /// <summary>
    /// Specific impulse by mass, Ns/kg
    /// </summary>
    public float ImpulseByMass => Constants.g * Impulse;

    /// <summary>
    /// Specific impulse, s (1 atm)
    /// </summary>
    [Label("Specific impulse, s (1 atm)")]
    public float ImpulseAtOneAtmosphere;

    /// <summary>
    /// Specific impulse by mass, Ns/kg
    /// </summary>
    public float ImpulseByMassAtOneAtmosphere => Constants.g * ImpulseAtOneAtmosphere;

    [Label("No gimbal")]
    public bool Fixed;

    public bool RadialMountedOnly;

    public FuelType Fuel;
}
