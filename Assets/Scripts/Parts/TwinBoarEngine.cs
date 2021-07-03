using UnityEngine;

[CreateAssetMenu(fileName = "NewTwinBoarEngine", menuName = "Parts/TwinBoarEngine")]
public class TwinBoarEngine : Engine
{
    /// <summary>
    /// Fuel mass tank, kg
    /// </summary>
    [Label("Fuel Mass Tank, kg")]
    public float FuelMassTank;
}
