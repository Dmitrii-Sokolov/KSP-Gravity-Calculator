using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VacuumIterator
{
    public struct Assembly
    {
        public Engine Engine;
        public int EnginesCount;
        public int StagesCount;
        public float[] FuelTanks;

        public float Mass;
        public float Cost;
        public float Time;
    }

    private const int mMaximumEnginesCount = 9;
    private const int mMaximumStagesCount = 9;
    private const int mMaximumAssembliesOnScreen = 30;
    private const int mGoodTime = 60;

    private Decoupler mStraightDecoupler;

    public float Payload { get; set; } = 5000f;
    public float DeltaV { get; set; } = 2000f;
    public List<Technology> Technologies { get; set; } = new List<Technology>();
    public bool UseAllTechnologies { get; set; } = true;

    public List<Assembly> Assemblies { get; private set; } = new List<Assembly>();

    public void Calculate()
    {
        Assemblies.Clear();

        if (UseAllTechnologies)
            Technologies = Technology.GetAll().ToList();

        mStraightDecoupler = Part.GetAll<Decoupler>().FirstOrDefault(p => p.Alias == "TD-12");

        FillAssembliesList();

        Debug.Log($"VacuumIterator  {Assemblies.Count}");
        Assemblies = Assemblies.OrderBy(set => set.Time).Take(mMaximumAssembliesOnScreen).ToList();
    }

    private void FillAssembliesList()
    {
        var engines = Technologies.SelectMany(tech => tech.Parts.OfType<Engine>()).Where(engine => !(engine is SolidFuelEngine));
        foreach (var engine in engines)
        {
            var criticalDeltaV = engine.Impulse * Constants.g * Mathf.Log(Constants.FuelMassToFuelTankMass / (Constants.FuelMassToFuelTankMass - 1));
            var minimumEnginesCount = engine.RadialMountedOnly ? 2 : 1;
            for (var enginesCount = minimumEnginesCount; enginesCount <= mMaximumEnginesCount; enginesCount++)
            {
                var minimumStagesCount = Mathf.CeilToInt(DeltaV / criticalDeltaV);
                for (var stagesCount = minimumStagesCount; stagesCount <= mMaximumStagesCount; stagesCount++)
                {
                    TryUseEngines(engine, enginesCount, stagesCount);
                    //TODO ??? Нужны отсечения
                }
            }
        }
    }

    private void TryUseEngines(Engine engine, int enginesCount, int stagesCount)
    {
        var assembly = new Assembly()
        {
            Engine = engine,
            EnginesCount = enginesCount,
            StagesCount = stagesCount,
            Mass = Payload + enginesCount * engine.Mass,
            Cost = enginesCount * engine.Cost,
            FuelTanks = new float[stagesCount]
        };

        var stageDeltaV = DeltaV / stagesCount;
        var exp = Mathf.Exp(stageDeltaV / (engine.Impulse * Constants.g));
        var multiplier = (exp - 1f) / (Constants.FuelMassToFuelTankMass + exp - Constants.FuelMassToFuelTankMass * exp);
        if (multiplier < 0f)
            return;

        for (var stage = 0; stage < stagesCount; stage++)
        {
            var fuelMass = assembly.Mass * multiplier;
            //TODO Учесть стоимость разных видов топлива
            //TODO Округлить топливо (разные виды по-разному)
            var fuelTankMass = fuelMass * Constants.FuelMassToFuelTankMass;
            var fuelCost = fuelMass * Constants.FuelMassToCost;
            assembly.Mass += fuelTankMass + mStraightDecoupler.Mass;
            assembly.Cost += fuelCost + mStraightDecoupler.Cost;
            assembly.Time += fuelTankMass / (enginesCount * engine.FuelConsumption);
            assembly.FuelTanks[stage] = fuelTankMass;
        }

        //TODO Проверить, что считается корректно
        foreach (var good in Assemblies)
        {
            if (assembly.Mass > good.Mass && 
                assembly.Cost > good.Cost && 
                (assembly.Time > good.Time || assembly.Time < mGoodTime && good.Time < mGoodTime))
                return;
        }
        Assemblies.Add(assembly);
    }
}
