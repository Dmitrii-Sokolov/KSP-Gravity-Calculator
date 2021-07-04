using UnityEngine;
using UnityEngine.UI;

public class FuelDrawer : MonoBehaviour
{
    [SerializeField] private Text mLiquidFuelTankMass;
    [SerializeField] private Text mDeltaV;

    public void Init(float fuelTankMass, float deltaV)
    {
        mLiquidFuelTankMass.text = fuelTankMass.ToString("N", Locale.Format) + "kg";
        mDeltaV.text = deltaV.ToString("N", Locale.Format) + "m/s";
    }
}
