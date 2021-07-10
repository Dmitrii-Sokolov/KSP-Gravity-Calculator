using System;
using System.Linq;
using UnityEngine;

public class LiquidSolidIterator : IteratorBase
{
    protected override void FillAssembliesList()
    {
        var engines0 = Technologies.SelectMany(tech => tech.Parts.OfType<SolidFuelEngine>());
        var engines1 = Technologies.SelectMany(tech => tech.Parts.OfType<Engine>()).Where(engine => !engine.Fixed && engine.Fuel == FuelType.RocketPropellant || engine.Fuel == FuelType.LiquidFuel);

        foreach (var engine0 in engines0)
        {
            foreach (var engine1 in engines1)
                TryUseEngines(engine0, engine1, Payload);
        }
    }

    private void TryUseEngines(SolidFuelEngine engine0, Engine engine1, float payload)
    {
        var assembly = new LiquidSolidTwoStageEngineAssembly();
        assembly.Engine0 = engine0;
        assembly.Engine1 = engine1;

        var minimumSecondStageEnginesCount = Mathf.CeilToInt(payload / (engine1.ThrustVacuum / Constants.MinAcceleration - engine1.Mass));
        minimumSecondStageEnginesCount = Mathf.Max(minimumSecondStageEnginesCount, engine1.RadialMountedOnly ? 2 : 1);
        for (var count1 = minimumSecondStageEnginesCount; count1 <= mMaximumEnginesPerStage; count1++)
        {
            var thrust1 = count1 * engine1.ThrustVacuum;
            var engines1Mass = count1 * engine1.Mass;
            var liquidFuelConsumption = count1 * engine1.FuelConsumption;
            var mass1Start = thrust1 / Constants.MinAcceleration;
            var reservedMass1 = mass1Start - engines1Mass - payload;
            var (liquidFuelMass1, liquidFuelTankMass1, liquidFuelCost1) = Constants.SplitFuelTank(reservedMass1, engine1.Fuel);

            if (engine1 is TwinBoarEngine twinBoar && twinBoar.FuelMassTank * count1 > liquidFuelTankMass1)
                continue;

            var mass1End = mass1Start - liquidFuelMass1;
            var stage1Cost = count1 * engine1.Cost + liquidFuelCost1;

            assembly.LiquidFuelMass1 = liquidFuelMass1;
            assembly.LiquidFuelTankMass1 = liquidFuelTankMass1;
            assembly.Engine1Count = count1;
            assembly.Time1 = liquidFuelMass1 / liquidFuelConsumption;
            assembly.DeltaV1 = engine1.Impulse * Constants.g * Mathf.Log(mass1Start / mass1End);

            var count0Min  = Mathf.CeilToInt(
                -(Constants.MinAcceleration * mass1Start - count1 * engine1.ThrustAtOneAtmosphere) /
                (Constants.MinAcceleration * (engine0.Mass + engine0.FuelMass) - engine0.ThrustAtOneAtmosphere));

            //TODO Minor Merge with LiquidIterator

            for (var count0 = count0Min; count0 <= mMaximumEnginesPerStage; count0++)
            {
                var decouplerCount = 2;
                var decoupler = mRadialDecoupler;

                if (count0 == 5 || count0 == 7)
                {
                    continue;
                }
                else if (count0 == 1)
                {
                    if (count1 == 1)
                        continue;

                    decouplerCount = 1;
                    decoupler = mStraightDecoupler;
                }
                else if (count0 == 3 || count0 == 9)
                {
                    decouplerCount = 3;
                }

                var solidFuelMass = count0 * engine0.FuelMass;
                var engines0Mass = count0 * engine0.Mass + decouplerCount * decoupler.Mass;
                var stage0MassMin = mass1Start + engines0Mass + solidFuelMass;

                var thrust0Max = count0 * engine0.ThrustAtOneAtmosphere + count1 * engine1.ThrustAtOneAtmosphere;
                var liquidFuelTankMass0AtMaxThrust = engine0.BurnTime * liquidFuelConsumption * Constants.FuelMassToTankMass(engine1.Fuel);
                var acceleration0Max = thrust0Max / (stage0MassMin + liquidFuelTankMass0AtMaxThrust);

                var rate = acceleration0Max < Constants.MinAcceleration
                    ? 1f
                    : GetQuadraticEquationRoot(
                        count0 * engine0.ThrustAtOneAtmosphere,
                        count1 * engine1.ThrustAtOneAtmosphere - Constants.MinAcceleration * stage0MassMin,
                        -Constants.MinAcceleration * liquidFuelConsumption * Constants.FuelMassToTankMass(engine1.Fuel) * engine0.BurnTime,
                        0f,
                        1f);

                var thrust = rate * count0 * engine0.ThrustAtOneAtmosphere + count1 * engine1.ThrustAtOneAtmosphere;
                var mass0Start = thrust / Constants.MinAcceleration;
                var reservedMass0 = mass0Start - stage0MassMin;
                var (liquidFuelMass0, liquidFuelTankMass0, liquidFuelCost0) = Constants.SplitFuelTank(reservedMass0, engine1.Fuel);
                var mass0End = mass0Start - liquidFuelMass0 - solidFuelMass;
                var stage0Cost = count0 * engine0.Cost + liquidFuelCost0 + decouplerCount * decoupler.Cost;
                var impulse0 = (solidFuelMass * count0 * engine0.ImpulseAtOneAtmosphere + liquidFuelMass0 * engine1.ImpulseAtOneAtmosphere) /
                    (solidFuelMass * count0 + liquidFuelMass0);

                assembly.Time0 = engine0.BurnTime / rate;
                assembly.DeltaV0 = impulse0 * Constants.g * Mathf.Log(mass0Start / mass0End);
                assembly.Cost = stage0Cost + stage1Cost;
                assembly.DeltaV = assembly.DeltaV0 + assembly.DeltaV1;
                assembly.Engine0Rate = rate;
                assembly.LiquidFuelMass0 = liquidFuelMass0;
                assembly.LiquidFuelTankMass0 = liquidFuelTankMass0;

                assembly.Engine0Count = count0;

                if (IsDeltaVEnough(assembly.DeltaV0, assembly.DeltaV) && IsCheep(assembly))
                {
                    BestCost = Mathf.Min(BestCost, assembly.Cost);
                    Assemblies.Add(assembly);
                };
            }
        }
    }

    private static float GetQuadraticEquationRoot(float a, float b, float c, float min, float max)
    {
        var d = b * b - 4f * a * c;
        var d12 = Mathf.Sqrt(d);
        var x0 = (-b - d12) / (2f * a);
        var x1 = (-b + d12) / (2f * a);
        return min < x0 && x0 < max && !(min < x1 && x1 < max)
            ? x0
            : !(min < x0 && x0 < max) && min < x1 && x1 < max
                ? x1 
                : throw new ArgumentException($"Values : {x0} {x1}");
    }
}
