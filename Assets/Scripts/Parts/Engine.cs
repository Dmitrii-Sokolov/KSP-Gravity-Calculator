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
    /// Specific impulse at vacuum, s
    /// </summary>
    [Label("Specific impulse, s")]
    public float Impulse;

    /// <summary>
    /// Specific impulse, s (1 atm)
    /// </summary>
    [Label("Specific impulse, s (1 atm)")]
    public float ImpulseAtOneAtmosphere;

    [Label("No gimbal")]
    public bool Fixed;

    public bool RadialMountedOnly;

    public FuelType Fuel;

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
    public float FuelConsumption => ThrustVacuum / (Constants.g * Impulse);

    /// <summary>
    /// Very approximate calculation
    /// </summary>
    public float GetImpulse(float atmosphere)
    {
        return Mathf.Lerp(Impulse, ImpulseAtOneAtmosphere, atmosphere);
    }

    /// <summary>
    /// Very approximate calculation
    /// </summary>
    public float GetThrust(float atmosphere)
    {
        return ThrustVacuum * GetImpulse(atmosphere) / Impulse;
    }

    /// <summary>
    /// Very approximate calculation
    /// </summary>
    public float GetFuelConsumptionImpulse(float atmosphere)
    {
        return FuelConsumption * GetImpulse(atmosphere);
    }
}
