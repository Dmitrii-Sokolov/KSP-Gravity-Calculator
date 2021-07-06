public struct VacuumAssembly
{
    public Engine Engine;
    public int EnginesCount;
    public int StagesCount;
    public (float FuelMass, float TankMass, float DeltaV)[] Stages;

    public float Mass;
    public float Cost;
    public float Time;
}
