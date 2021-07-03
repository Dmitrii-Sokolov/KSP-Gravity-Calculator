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

    /// <summary>
    /// Thrust at vacuum, N
    /// </summary>
    public float ThrustVacuum => 1000f * Thrust;

    /// <summary>
    /// Thrust at one atmosphere, N
    /// </summary>
    public float ThrustAtOneAtmosphere => ThrustVacuum * ImpulseAtOneAtmosphere / Impulse;

    /// <summary>
    /// Fuel consumption, kg/s
    /// </summary>
    public float FuelConsumption => 1000f * Thrust / ImpulseByMass;

    /// <summary>
    /// Fuel consumption impulse, kg
    /// </summary>
    public float FuelConsumptionImpulse => 1000f * Thrust / Constants.g;

    /// <summary>
    /// Specific impulse at vacuum, s
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
