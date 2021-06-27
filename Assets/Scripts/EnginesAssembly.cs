using System.Collections.Generic;
using UnityEngine;

public class EnginesAssembly
{
    public List<(Engine Engine, Stage Mode, float Rate, int Count)> EnginesSet = new List<(Engine Engine, Stage Mode, float Rate, int Count)>();

    /// <summary>
    /// kN
    /// </summary>
    /// <returns></returns>
    public float GetThrust(Stage stage)
    {
        var thrust = 0f;
        foreach (var set in EnginesSet)
            thrust += set.Mode.HasFlag(stage) ? set.Rate * set.Count * set.Engine.GetThrust(stage) : 0f;

        return thrust;
    }

    /// <summary>
    /// kg
    /// </summary>
    /// <returns></returns>  
    public float GetMaxMass(Stage stage)
    {
        return 1000f * GetThrust(stage) / Constants.MinTWR / Constants.g;
    }

    public float GetRocketFuelMassBySolidFuel(Stage stage)
    {
        var time = 0f;
        foreach (var set in EnginesSet)
        {
            if (set.Mode.HasFlag(stage) && set.Engine is SolidFuelEngine engine)
                time = Mathf.Max(time, engine.BurnTime / set.Rate);
        }

        var fuelMass = 0f;
        foreach (var set in EnginesSet)
        {
            if (set.Mode.HasFlag(stage) && !(set.Engine is SolidFuelEngine))
                fuelMass += time * set.Count * set.Rate * set.Engine.FuelConsumption;
        }

        return fuelMass;
    }

    //Нужно выровнять время работы твёрдотопливных
}
