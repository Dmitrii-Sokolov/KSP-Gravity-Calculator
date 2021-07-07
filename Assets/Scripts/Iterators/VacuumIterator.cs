using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VacuumIterator
{
    //TODO Medium Сделать динамическую сортировку/подсветку
    //TODO Medium Сделать эти параметры изменяемыми на лету
    //TODO Medium Вывести импульс, тягу и тягу связки и массу в таблицу, а также обе массы топлива + критический dv, импульс в атмосфере, 
    private const int mMaximumEnginesCount = 1;
    private const int mMaximumStagesCount = 1;
    private const int mMaximumAssembliesOnScreen = 60;

    private Decoupler mStraightDecoupler;

    public float Payload { get; set; } = 5000f;
    public float DeltaV { get; set; } = 2000f;
    public List<Technology> Technologies { get; set; } = new List<Technology>();
    public bool UseAllTechnologies { get; set; } = true;

    public List<VacuumAssembly> Assemblies { get; private set; } = new List<VacuumAssembly>();

    public void Calculate()
    {
        Assemblies.Clear();

        if (UseAllTechnologies)
            Technologies = Technology.GetAll().ToList();

        mStraightDecoupler = Part.GetAll<Decoupler>().FirstOrDefault(p => p.Alias == "TD-12");

        FillAssembliesList();

        Assemblies = Assemblies.OrderBy(set => -set.Engine.Impulse).Take(mMaximumAssembliesOnScreen).ToList();
    }

    private void FillAssembliesList()
    {
        //TODO Medium Придумать как отсечь ненужные варианты
        var engines = Technologies.SelectMany(tech => tech.Parts.OfType<Engine>()).Where(engine => !(engine is SolidFuelEngine));
        foreach (var engine in engines)
        {
            var tank = Constants.FuelMassToTankMass(engine.Fuel);
            var criticalDeltaV = engine.Impulse * Constants.g * Mathf.Log(tank / (tank - 1));
            var enginesCountMin = engine.RadialMountedOnly ? 2 : 1;
            var enginesCountMax = Mathf.Max(mMaximumEnginesCount, enginesCountMin);
            for (var enginesCount = enginesCountMin; enginesCount <= enginesCountMax; enginesCount++)
            {
                //Считаем, что на одну ступень не стоит нагружать более половины критической DeltaV
                //Разница между ступенью DeltaV / 2 и двумя ступенями по DeltaV / 4 составит 13% по массе топлива
                //На практике, число ступеней стоит увеличивать до предела опираясь на максимально доступную сложность сборки и желаемую комфортностью управления
                var stagesCountMin = Mathf.CeilToInt(2f * DeltaV / criticalDeltaV);
                var stagesCountMax = Mathf.Max(stagesCountMin, mMaximumStagesCount);
                for (var stagesCount = stagesCountMin; stagesCount <= stagesCountMax; stagesCount++)
                {
                    var assembly = new VacuumAssembly()
                    {
                        Engine = engine,
                        EnginesCount = enginesCount,
                        StagesCount = stagesCount,
                        Mass = Payload + enginesCount * engine.Mass,
                        Cost = enginesCount * engine.Cost,
                        Stages = new (float FuelMass, float tankMass, float DeltaV)[stagesCount]
                    };

                    var stageDeltaV = DeltaV / stagesCount;
                    var exp = Mathf.Exp(stageDeltaV / (engine.Impulse * Constants.g));
                    var multiplier = (exp - 1f) / (tank + exp - tank * exp);
                    if (multiplier < 0f)
                        return;

                    for (var stage = 0; stage < stagesCount; stage++)
                    {
                        var (fuelMass, fuelTankMass, fuelCost) = Constants.SplitFuel(assembly.Mass * multiplier, engine.Fuel);
                        var deltaV = engine.Impulse * Constants.g * Mathf.Log((assembly.Mass + fuelTankMass) / (assembly.Mass + fuelTankMass - fuelMass)); 
                        assembly.Mass += fuelTankMass + mStraightDecoupler.Mass;
                        assembly.Cost += fuelCost + mStraightDecoupler.Cost;
                        assembly.Time += fuelMass / (enginesCount * engine.FuelConsumption);
                        assembly.Stages[stage] = (fuelMass, fuelTankMass, deltaV);
                    }

                    //if (!Assemblies.Any(good =>
                    //    assembly.Mass > good.Mass &&
                    //    assembly.Cost > good.Cost &&
                    //    (assembly.Time > good.Time || assembly.Time < mGoodTime && good.Time < mGoodTime)))
                    Assemblies.Add(assembly);
                }
            }
        }
    }
}
