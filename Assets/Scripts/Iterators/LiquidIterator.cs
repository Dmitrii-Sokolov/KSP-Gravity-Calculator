using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LiquidIterator : IteratorBase
{
    private IEnumerable<Engine> mEngines;
    private IEnumerable<Engine> mTopEngines;

    protected override void FillAssembliesList()
    {
        mEngines = Technologies.SelectMany(tech => tech.Parts.OfType<Engine>()).Where(engine => engine.Fuel == FuelType.RocketPropellant || engine.Fuel == FuelType.LiquidFuel);
        mTopEngines = mEngines.Where(engine => !engine.Fixed);

        var baseAssembly = new LiquidEngineAssembly(Payload);
        TryUseEngines(ref baseAssembly);
    }

    private void TryUseEngines(ref LiquidEngineAssembly assembly)
    {
        foreach (var engine in assembly.Stages.Count == 0 ? mTopEngines : mEngines)
        {
            //—читаем, что добрались до дна отмосферы
            TryAddStage(ref assembly, engine, true);

            //—читаем, что мы ещЄ в вакууме
            TryAddStage(ref assembly, engine, false);
        }
    }

    private void TryAddStage(ref LiquidEngineAssembly assembly, Engine engine, bool isAtmosphereExists)
    {
        var atmosphere = !isAtmosphereExists
            ? 0f
            : assembly.Stages.Count == 0
                ? 0.5f
                : 1f;

        var minimumEnginesCount = Mathf.CeilToInt(assembly.Mass / (engine.GetThrust(atmosphere) / Constants.MinAcceleration - engine.Mass));
        minimumEnginesCount = Mathf.Max(minimumEnginesCount, 1);
        for (var count = minimumEnginesCount; count < mMaximumEnginesPerStage + 1; count++)
        {
            if (engine.RadialMountedOnly && count == 1)
                continue;

            var decouplerCount = 2;
            var decoupler = mRadialDecoupler;

            if (count == 5 || count == 7)
            {
                continue;
            }
            else if (count == 1)
            {
                //TODO ”честь систему 1+1 и другие частные случаи
                //TODO UI дл€ трЄх ступеней

                decouplerCount = 1;
                decoupler = mStraightDecoupler;
            }
            else if (count == 3 || count == 9)
            {
                decouplerCount = 3;
            }

            var enginesMass = count * engine.Mass + decouplerCount * decoupler.Mass;
            var thrust = count * engine.GetThrust(atmosphere);
            var liquidFuelConsumption = count * engine.FuelConsumption;
            var liquidFuelConsumptionImpulse = count * engine.GetFuelConsumptionImpulse(atmosphere);

            foreach (var stage in assembly.Stages)
            {
                thrust += stage.EngineCount * stage.Engine.GetThrust(atmosphere);
                liquidFuelConsumption += stage.EngineCount * stage.Engine.FuelConsumption;
                liquidFuelConsumptionImpulse += stage.EngineCount * stage.Engine.GetFuelConsumptionImpulse(atmosphere);
            }

            var massStart = thrust / Constants.MinAcceleration;
            var liquidFuelTankMass = massStart - enginesMass - assembly.Mass;
            if (engine is TwinBoarEngine twinBoar && twinBoar.FuelMassTank * count > liquidFuelTankMass)
                continue;

            var liquidFuelMass = liquidFuelTankMass / Constants.FuelMassToFuelTankMass;
            var stageCost = count * engine.Cost + liquidFuelMass * Constants.FuelMassToCost + decouplerCount * decoupler.Cost;

            var newAssembly = assembly.GetCopy();
            newAssembly.Cost += stageCost;

            if (!IsCheep(newAssembly))
                continue;

            var stageTime = liquidFuelMass / liquidFuelConsumption;
            var stageImpulse = liquidFuelConsumptionImpulse / liquidFuelConsumption;
            var massEnd = massStart - liquidFuelMass;
            var stageDeltaV = stageImpulse * Constants.g * Mathf.Log(massStart / massEnd);

            newAssembly.DeltaV += stageDeltaV;
            newAssembly.Mass = massStart;
            newAssembly.Stages.Add(new LiquidEngineAssembly.LiquidEngineStage()
            {
                Engine = engine,
                EngineCount = count,
                LiquidFuelTankMass = liquidFuelTankMass,
                DeltaV = stageDeltaV,
                Time = stageTime,
            });

            if (isAtmosphereExists)
            {
                if (IsDeltaVEnough(stageDeltaV, newAssembly.DeltaV))
                {
                    BestCost = Mathf.Min(BestCost, newAssembly.Cost);
                    Assemblies.Add(newAssembly);
                };
            }
            else
            {
                TryUseEngines(ref newAssembly);
            }
        }
    }
}
