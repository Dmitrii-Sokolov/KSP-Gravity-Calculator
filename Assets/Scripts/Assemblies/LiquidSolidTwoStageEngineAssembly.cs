public struct LiquidSolidTwoStageEngineAssembly : IEngineAssembly
{
    public SolidFuelEngine Engine0;
    public int Engine0Count;
    public float Engine0Rate;

    public Engine Engine1;
    public int Engine1Count;

    public float LiquidFuelTankMass0;
    public float LiquidFuelTankMass1;

    public float DeltaV0;
    public float DeltaV1;

    public float Time0;
    public float Time1;

    public float DeltaV { get; set; }
    public float Cost { get; set; }
}
