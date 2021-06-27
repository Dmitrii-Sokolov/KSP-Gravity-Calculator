using UnityEngine;

public enum FuelType
{
    [InspectorName("Rocket Propellant")]
    RocketPropellant,

    Monopropellant,

    [InspectorName("Liquid Fuel")]
    LiquidFuel,

    [InspectorName("Solid Fuel")]
    SolidFuel,

    [InspectorName("Xenon Gas")]
    Xenon
}
