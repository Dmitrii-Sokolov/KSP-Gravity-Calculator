using UnityEngine;
using UnityEngine.UI;

public class FuelDrawer : MonoBehaviour
{
    [SerializeField] private Text mLiquidFuelMass;
    [SerializeField] private Text mLiquidFuelTankMass;
    [SerializeField] private Text mDeltaV;

    public void Init(float fuelMass, float fuelTankMass, float deltaV)
    {
        mLiquidFuelMass.text = fuelMass.ToString("N", Locale.Format) + "kg";
        mLiquidFuelTankMass.text = fuelTankMass.ToString("N", Locale.Format) + "kg";
        mDeltaV.text = deltaV.ToString("N", Locale.Format) + "m/s";
    }

    public void ShowFuelMass(bool mShowFuelMass)
    {
        mLiquidFuelMass.gameObject.SetActive(mShowFuelMass);
        mLiquidFuelTankMass.gameObject.SetActive(!mShowFuelMass);
    }
}
