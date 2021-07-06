using System;
using UnityEngine;

public static class Constants
{
    public const float g = 9.81f;
    public const float MinTWR = 1.5f;
    public const float MinAcceleration = MinTWR * g;
    public const float MinKerbinDeltaV = 3400f;

    public static readonly float[] FuelCapacities2 = new float[]
    {
        256000f,
        128000f,
        64000f,
        32000f,
        16000f,
        8000f,
        4000f,
        2000f,
        1000f,
        500f
    };
    
    public static float[] FuelCapacities3 = new float[]
    {
        72000f,
        36000f,
        18000f,
        9000f,
        4500f,
        2200f,
        1100f
    };

    public static (float Mass, float TankMass, float Cost)[] MonopropellantCapacities = new (float Mass, float TankMass, float Cost)[]
    {
        (80f, 100f, 200f),
        (480f, 560f, 330f),
        (3000f, 3400f, 1800f),
    };

    public static (float Mass, float TankMass, float Cost)[] LiquidFuelCapacities = new (float Mass, float TankMass, float Cost)[]
    {
        (2000f, 2250f, 550f),
    };

    public static (float Mass, float TankMass, float Cost)[] XenonCapacities = new (float Mass, float TankMass, float Cost)[]
    {
        (570f, 760f, 24300f),
        (72f, 96f, 3680f),
        (40f, 54f, 2220f),
    };

    public static float FuelMassToTankMass(FuelType type)
    {
        switch (type)
        {
            case FuelType.RocketPropellant:
            case FuelType.LiquidFuel:
                return 1.125f;

            case FuelType.Monopropellant:
                return 1.167f;

            case FuelType.Xenon:
                return 1.333f;

            case FuelType.SolidFuel:
                throw new ArgumentException(type.ToString());
            default:
                throw new NotImplementedException(type.ToString());
        }
    }

    public static (float Mass, float TankMass, float Cost) SplitFuelTank(float value, FuelType type)
    {
        return SplitFuel(value / FuelMassToTankMass(type), type);
    }
    
    public static (float Mass, float TankMass, float Cost) SplitFuel(float value, FuelType type)
    {
        switch (type)
        {
            case FuelType.RocketPropellant:
                //Просто разливаем по минимальным бакам, без перебора вариантов
                var massR = value < 500f ? Mathf.Ceil(value / 200f) * 200f : Mathf.Ceil(value / 500f) * 500f;
                return (massR, massR * 1.125f, massR * 0.2f);

            case FuelType.Monopropellant:
                //Просто разливаем по самому ходовому баку, без перебора вариантов
                var countM = Mathf.Ceil(value / 480f);
                return (countM * 480f, countM * 560f, countM * 330f);

            case FuelType.LiquidFuel:
                //Просто разливаем по самому ходовому баку, без перебора вариантов
                var countL = Mathf.Ceil(value / 2000f);
                return (countL * 2000f, countL * 2250, countL * 550f);

            case FuelType.Xenon:
                var (xenonMass, xenonTankMass, xenonCost) = (0f, 0f, 0f);
                for (var xTank = 0; xTank < XenonCapacities.Length; xTank++)
                {
                    var count = Mathf.RoundToInt(value / XenonCapacities[xTank].Mass);
                    value -= count * XenonCapacities[xTank].Mass;
                    xenonMass += count * XenonCapacities[xTank].Mass;
                    xenonTankMass += count * XenonCapacities[xTank].TankMass;
                    xenonCost += count * XenonCapacities[xTank].Cost;
                    if (value < 0f)
                        break;
                }
                xenonMass = Mathf.Max(xenonMass, XenonCapacities[XenonCapacities.Length - 1].Mass);
                xenonTankMass = Mathf.Max(xenonTankMass, XenonCapacities[XenonCapacities.Length - 1].TankMass);
                xenonCost = Mathf.Max(xenonCost, XenonCapacities[XenonCapacities.Length - 1].Cost);
                return (xenonMass, xenonTankMass, xenonCost);

            case FuelType.SolidFuel:
                throw new ArgumentException(type.ToString());
            default:
                throw new NotImplementedException(type.ToString());
        }
    }
}
