using System.Collections.Generic;

public struct LiquidEngineAssembly : IEngineAssembly
{
    public struct LiquidEngineStage
    {
        public Engine Engine;
        public int EngineCount;
        public float LiquidFuelMass;
        public float LiquidFuelTankMass;
        public float DeltaV;
        public float Time;
    }

    public List<LiquidEngineStage> Stages;
    public float Mass;

    public float DeltaV { get; set; }
    public float Cost { get; set; }

    public LiquidEngineAssembly(float mass) : this()
    {
        Stages = new List<LiquidEngineStage>();
        Mass = mass;
        DeltaV = 0f;
        Cost = 0f;
    }

    public void AddDraft(Engine engine, int count)
    {
        Stages.Add(new LiquidEngineStage()
        {
            Engine = engine,
            EngineCount = count,
        });
    }

    public void SetCurrentStageData(float liquidFuelMass, float liquidFuelTankMass, float stageDeltaV, float stageTime)
    {
        var stage = Stages[Stages.Count - 1];
        stage.LiquidFuelMass = liquidFuelMass;
        stage.LiquidFuelTankMass = liquidFuelTankMass;
        stage.DeltaV = stageDeltaV;
        stage.Time = stageTime;
        Stages[Stages.Count - 1] = stage;
    }

    public LiquidEngineAssembly GetCopy()
    {
        return new LiquidEngineAssembly()
        {
            Stages = new List<LiquidEngineStage>(Stages),
            Mass = Mass,
            DeltaV = DeltaV,
            Cost = Cost
        };
    }
}
