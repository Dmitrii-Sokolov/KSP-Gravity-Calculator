using UnityEngine;

public enum FuelType
{
    [InspectorName("Rocket Propellant")]
    RocketPropellant,

    Monopropellant,

    [InspectorName("Liquid Fuel")]
    LiquidFuel,

    [InspectorName("Xenon Gas")]
    Xenon
}
