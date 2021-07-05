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
        var averageAtmosphere = isAtmosphereExists ? (assembly.Stages.Count == 0 ? 0.5f : 1f) : 0f;
        var startAtmosphere = isAtmosphereExists ? 1f : 0f;

        var minimumEnginesCount = Mathf.CeilToInt(assembly.Mass / (engine.GetThrust(startAtmosphere) / Constants.MinAcceleration - engine.Mass));
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
                decouplerCount = 1;
                decoupler = mStraightDecoupler;
            }
            else if (count == 3 || count == 9)
            {
                decouplerCount = 3;
            }

            var newAssembly = assembly.GetCopy();
            newAssembly.AddDraft(engine, count);

            var thrust = 0f;
            var liquidFuelConsumption = 0f;
            var liquidFuelConsumptionImpulse = 0f;
            var oneEnginePerStage = false;

            for (var i = newAssembly.Stages.Count - 1; i >= 0; i--)
            {
                var stage = newAssembly.Stages[i];
                //≈сли более чем на одной ступени ровно один двигатель, то работает только сама€ нижн€€ из таких
                if (stage.EngineCount == 1)
                {
                    if (oneEnginePerStage)
                        continue;
                    oneEnginePerStage = true;
                }

                thrust += stage.EngineCount * stage.Engine.GetThrust(startAtmosphere);
                liquidFuelConsumption += stage.EngineCount * stage.Engine.FuelConsumption;
                liquidFuelConsumptionImpulse += stage.EngineCount * stage.Engine.GetFuelConsumptionImpulse(averageAtmosphere);
            }

            var massStart = thrust / Constants.MinAcceleration;
            var enginesMass = count * engine.Mass + decouplerCount * decoupler.Mass;
            var liquidFuelTankMass = massStart - enginesMass - newAssembly.Mass;

            if (engine is TwinBoarEngine twinBoar && twinBoar.FuelMassTank * count > liquidFuelTankMass)
                continue;

            var liquidFuelMass = liquidFuelTankMass / Constants.FuelMassToFuelTankMass;
            var stageCost = count * engine.Cost + liquidFuelMass * Constants.FuelMassToCost + decouplerCount * decoupler.Cost;
            newAssembly.Cost += stageCost;

            if (!IsCheep(newAssembly))
                continue;

            var stageTime = liquidFuelMass / liquidFuelConsumption;
            var stageImpulse = liquidFuelConsumptionImpulse / liquidFuelConsumption;
            var massEnd = massStart - liquidFuelMass;
            var stageDeltaV = stageImpulse * Constants.g * Mathf.Log(massStart / massEnd);

            newAssembly.DeltaV += stageDeltaV;
            newAssembly.Mass = massStart;
            newAssembly.SetCurrentStageData(liquidFuelTankMass, stageDeltaV, stageTime);

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
                if (IsDeltaVEnough(stageDeltaV))
                    TryUseEngines(ref newAssembly);
            }
        }
    }
}
