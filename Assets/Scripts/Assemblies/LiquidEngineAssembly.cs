using System.Collections.Generic;

public struct LiquidEngineAssembly : IEngineAssembly
{
    public struct LiquidEngineStage
    {
        public Engine Engine;
        public int EngineCount;
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
