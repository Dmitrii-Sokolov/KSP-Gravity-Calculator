using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LiquidIterator : IteratorBase
{
    private IEnumerable<Engine> mEngines;
    private IEnumerable<Engine> mTopEngines;

    public bool UseOneStage { get; set; } = false;

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
            //We believe that we have reached the bottom of the atmosphere
            TryAddStage(ref assembly, engine, true);

            //We believe that we are still in a vacuum
            if (!UseOneStage)
                TryAddStage(ref assembly, engine, false);
        }
    }

    private void TryAddStage(ref LiquidEngineAssembly assembly, Engine engine, bool isAtmosphereExists)
    {
        //TODO Why cool powerful motors like Mammoth and Vector are not included in the selection?
        //TODO Try adding a different engines

        var averageAtmosphere = isAtmosphereExists ? (assembly.Stages.Count == 0 ? 0.5f : 1f) : 0f;
        var startAtmosphere = isAtmosphereExists ? 1f : 0f;

        var minimumEnginesCount = Mathf.CeilToInt(assembly.Mass / (engine.GetThrust(startAtmosphere) / Constants.MinAcceleration - engine.Mass));
        minimumEnginesCount = Mathf.Max(minimumEnginesCount, engine.RadialMountedOnly ? 2 : 1);
        for (var count = minimumEnginesCount; count <= mMaximumEnginesPerStage; count++)
        {
            var decouplerCount = 2;
            var decoupler = mRadialDecoupler;

            if (assembly.Stages.Count == 0)
            {
                decouplerCount = 0;
            }
            else if (count == 5 || count == 7)
            {
                decouplerCount = count;
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
                //If there is exactly one engine in more than one stage, then only the lowest of such engines operates.
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
            var reservedMass = massStart - enginesMass - newAssembly.Mass;
            var (fuelMass, tankMass, fuelCost) = Constants.SplitFuelTank(reservedMass, engine.Fuel);

            if (engine is TwinBoarEngine twinBoar && twinBoar.FuelMassTank * count > tankMass)
                continue;

            var stageCost = count * engine.Cost + fuelCost + decouplerCount * decoupler.Cost;
            newAssembly.Cost += stageCost;

            if (!IsCheep(newAssembly))
                continue;

            var stageTime = fuelMass / liquidFuelConsumption;
            var stageImpulse = liquidFuelConsumptionImpulse / liquidFuelConsumption;
            var massEnd = massStart - fuelMass;
            var stageDeltaV = stageImpulse * Constants.g * Mathf.Log(massStart / massEnd);

            newAssembly.DeltaV += stageDeltaV;
            newAssembly.Mass = massStart;
            newAssembly.SetCurrentStageData(fuelMass, tankMass, stageDeltaV, stageTime);

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
