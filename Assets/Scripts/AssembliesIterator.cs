using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;

public class AssembliesIterator
{
    private const float mMaxMillisecondsPerFrame = 20;
    private const float mMaxAttemptsPerFrame = 10;
    //private const float mMaximumCostExcess = 1.2f;
    private const float mMaximumDeltaVLack = 0.90f;
    private const int mMaximumEnginesPerStage = 8;

    private CancellationTokenSource mCancellationTokenSource = new CancellationTokenSource();
    private List<Engine> mEngines = new List<Engine>();
    private List<Engine> mLiquidFuelEngines = new List<Engine>();
    private List<SolidFuelEngine> mSolidFuelEngines = new List<SolidFuelEngine>();
    private Decoupler mDecoupler;

    public float Payload { get; set; } = 1000f;
    public List<Technology> Technologies { get; set; } = new List<Technology>();
    public bool UseAllTechnologies { get; set; } = true;
    public bool UseRandomization { get; set; } = false;

    public ObservableCollection<LiquidSolidClassicEngineAssembly> Assemblies { get; } = new ObservableCollection<LiquidSolidClassicEngineAssembly>();
    public float BestCost { get; set; }

    public void StopIterating()
    {
        mCancellationTokenSource?.Cancel();
        mCancellationTokenSource?.Dispose();
        mCancellationTokenSource = new CancellationTokenSource();
    }

    public async void StartIterating()
    {
        StopIterating();
        CheckEngines();
        await Iterating(mCancellationTokenSource.Token);
        //await UniTask.Run(() => Iterating(mCancellationTokenSource.Token));
    }

    private void CheckEngines()
    {
        Assemblies.Clear();
        BestCost = float.PositiveInfinity;

        if (UseAllTechnologies)
            Technologies = Technology.GetAll().ToList();

        mEngines.Clear();
        foreach (var tech in Technologies)
            mEngines.AddRange(tech.Parts.OfType<Engine>());

        if (UseRandomization)
        {
            var random = new System.Random();
            mEngines = mEngines.OrderBy(_ => random.Next()).ToList();
        }

        mLiquidFuelEngines.Clear();
        mLiquidFuelEngines.AddRange(mEngines.Where(engine => !engine.Fixed && !(engine is SolidFuelEngine)));

        mSolidFuelEngines.Clear();
        mSolidFuelEngines.AddRange(mEngines.OfType<SolidFuelEngine>());

        mDecoupler = Part.GetAll<Decoupler>().FirstOrDefault(p => p.Alias == "TT-70");
    }

    private async UniTask Iterating(CancellationToken token)
    {
        var counter = 0;
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        foreach (var engine0 in mSolidFuelEngines)
        {
            foreach (var engine1 in mLiquidFuelEngines)
            {
                TryUseEngines(engine0, engine1, Payload);

                if (stopwatch.ElapsedMilliseconds > mMaxMillisecondsPerFrame || counter++ > mMaxAttemptsPerFrame)
                {
                    stopwatch.Stop();
                    stopwatch.Reset();
                    counter = 0;
                    await UniTask.Yield();
                    stopwatch.Start();
                    token.ThrowIfCancellationRequested();
                }
            }
        }
    }

    private void TryUseEngines(SolidFuelEngine engine0, Engine engine1, float payload)
    {
        var assembly = new LiquidSolidClassicEngineAssembly();
        assembly.Engine0 = engine0;
        assembly.Engine1 = engine1;

        var minimumSecondStageEnginesCount = Mathf.CeilToInt(Payload / (engine1.ThrustVacuum / (Constants.MinTWR * Constants.g) - engine1.Mass));
        for (var count1 = minimumSecondStageEnginesCount; count1 < mMaximumEnginesPerStage; count1++)
        {
            var thrust1 = count1 * engine1.ThrustVacuum;
            var engines1mass = count1 * engine1.Mass;
            var liquidFuelConsumption = count1 * engine1.FuelConsumption;
            var mass1Start = thrust1 / (Constants.MinTWR * Constants.g);
            var liquidFuelTankMass1 = mass1Start - engines1mass - payload;
            var liquidFuelMass1 = liquidFuelTankMass1 / Constants.FuelMassToFuelTankMass;
            var mass1End = mass1Start - liquidFuelMass1;
            var stage1Cost = count1 * engine1.Cost + liquidFuelMass1 * Constants.FuelMassToCost;

            if (liquidFuelMass1 < 0f)
                throw new ArgumentException();

            assembly.LiquidFuelTankMass1 = liquidFuelTankMass1;
            assembly.Engine1Count = count1;
            assembly.Time1 = liquidFuelMass1 / liquidFuelConsumption;
            assembly.DeltaV1 = engine1.Impulse * Constants.g * Mathf.Log(mass1Start / mass1End);

            var count0Min  = Mathf.CeilToInt(
                -(Constants.MinTWR * Constants.g * mass1Start - count1 * engine1.ThrustAtOneAtmosphere) /
                (Constants.MinTWR * Constants.g * engine0.Mass - engine0.ThrustAtOneAtmosphere));
            count0Min = Mathf.Max(count0Min, count1 == 1 ? 2 : 1);

            for (var count0 = count0Min; count0 < mMaximumEnginesPerStage; count0++)
            {
                var solidFuelMass = count0 * engine0.FuelMass;
                var maxThrust0 = count0 * engine0.ThrustAtOneAtmosphere + count1 * engine1.ThrustAtOneAtmosphere;
                var engines0Mass = count0 * engine0.Mass + 2f * mDecoupler.Mass;

                var liquidFuelTankMass0Pattern = liquidFuelConsumption * engine0.BurnTime * Constants.FuelMassToFuelTankMass;
                var mass0StartPattern = mass1Start + engines0Mass + solidFuelMass + liquidFuelTankMass0Pattern;
                var patternTWR = maxThrust0 / (mass0StartPattern * Constants.g);
                if (patternTWR > Constants.MinTWR)
                {
                    //Если тяга слишком большая, то можно уменьшить режим твёрдотопливных, увеличив время и увеличив запас ракетного топлива
                    //Решаем квадратное уравнение
                    var rate = GetQuadraticEquationRoot(
                        count0 * engine0.ThrustAtOneAtmosphere,
                        count1 * engine1.ThrustAtOneAtmosphere - Constants.MinTWR * (mass1Start + engines0Mass + solidFuelMass),
                        Constants.MinTWR * liquidFuelConsumption * engine0.BurnTime,
                        0f,
                        1f);

                    var mass0Start = (rate * count0 * engine0.ThrustAtOneAtmosphere + count1 * engine1.ThrustAtOneAtmosphere) / (Constants.MinTWR * Constants.g);
                    var liquidFuelTankMass0 = mass0Start - mass1Start - solidFuelMass - engines0Mass;
                    var liquidFuelMass0 = liquidFuelTankMass0 / Constants.FuelMassToFuelTankMass;

                    if (Mathf.Abs(liquidFuelMass0 - liquidFuelConsumption * engine0.BurnTime / rate) > 0.1f)
                        throw new ArgumentException();

                    var mass0End = mass0Start - liquidFuelMass0 - solidFuelMass;
                    var stage0Cost = count0 * engine0.Cost + liquidFuelMass0 * Constants.FuelMassToCost + 2f * mDecoupler.Cost;
                    var impulse0 = (solidFuelMass * count0 * engine0.ImpulseAtOneAtmosphere + liquidFuelMass0 * engine1.ImpulseAtOneAtmosphere) /
                        (solidFuelMass * count0 + liquidFuelMass0);

                    assembly.Time0 = engine0.BurnTime;
                    assembly.DeltaV0 = impulse0 * Constants.g * Mathf.Log(mass0Start / mass0End);
                    assembly.Cost = stage0Cost + stage1Cost;
                    assembly.DeltaV = assembly.DeltaV0 + assembly.DeltaV1;
                    assembly.Engine0Rate = rate;
                    assembly.LiquidFuelTankMass0 = liquidFuelTankMass0;
                }
                else
                {
                    //Тяга может быть недостаточной, тогда нужно уменьшить ракетное топливо, с нулевым запасом топлива тяги точно должно хватить
                    var mass0Start = maxThrust0 / (Constants.MinTWR * Constants.g);
                    var liquidFuelTankMass0 = mass0Start - mass1Start - solidFuelMass - engines0Mass;
                    var liquidFuelMass0 = liquidFuelTankMass0 / Constants.FuelMassToFuelTankMass;
                    var mass0End = mass0Start - liquidFuelMass0 - solidFuelMass;
                    var stage0Cost = count0 * engine0.Cost + liquidFuelMass0 * Constants.FuelMassToCost + 2f * mDecoupler.Cost;
                    var impulse0 = (solidFuelMass * count0 * engine0.ImpulseAtOneAtmosphere + liquidFuelMass0 * engine1.ImpulseAtOneAtmosphere) /
                        (solidFuelMass * count0 + liquidFuelMass0);

                    assembly.Time0 = engine0.BurnTime;
                    assembly.DeltaV0 = impulse0 * Constants.g * Mathf.Log(mass0Start / mass0End);
                    assembly.Cost = stage0Cost + stage1Cost;
                    assembly.DeltaV = assembly.DeltaV0 + assembly.DeltaV1;
                    assembly.Engine0Rate = 1f;
                    assembly.LiquidFuelTankMass0 = liquidFuelTankMass0;
                }

                assembly.Engine0Count = count0;
                CheckAssembly(assembly);
            }
        }
    }

    private void CheckAssembly(LiquidSolidClassicEngineAssembly assembly)
    {
        //if (assembly.DeltaV > Constants.MinTWR * mMaximumDeltaVLack && assembly.Cost < BestCost * mMaximumCostExcess)
        if (assembly.DeltaV > Constants.MinTWR * mMaximumDeltaVLack)
        {
            BestCost = Mathf.Min(BestCost, assembly.Cost);
            Assemblies.Add(assembly);
        };
    }

    private static float GetQuadraticEquationRoot(float a, float b, float c, float min, float max)
    {
        var d = b * b - 4 * a * c;
        var d12 = Mathf.Sqrt(d);
        var x0 = -b - d12;
        var x1 = -b + d12;
        return min < x0 && x0 < max && !(min < x1 && x1 < max)
            ? x0
            : !(min < x0 && x0 < max) && min < x1 && x1 < max
                ? x1 
                : throw new ArgumentException($"Values : {x0} {x1}");
    }
}
