using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssemblyDrawer : MonoBehaviour
{
    private List<FuelDrawer> mFuelDrawers;

    private bool mIsSelected = false;

    [SerializeField] private Color mNormal;
    [SerializeField] private Color mActive;

    [SerializeField] private EngineDrawer mEngineDrawer;
    [SerializeField] private Text mDeltaV;
    [SerializeField] private Text mCost;
    [SerializeField] private FuelDrawer mFuelDrawer;

    [SerializeField] private Button mButton;

    public void Init(IEngineAssembly assembly)
    {
        mFuelDrawers = new List<FuelDrawer>() { mFuelDrawer };
        mButton.onClick.AddListener(SwitchState);

        mDeltaV.text = assembly.DeltaV.ToString("N", Locale.Format) + "m/s";
        mCost.text = assembly.Cost.ToString("N", Locale.Format);

        switch (assembly)
        {
            case LiquidEngineAssembly lea:
                mEngineDrawer.Init(lea.Stages[0].EngineCount, lea.Stages[0].Engine);
                mFuelDrawer.Init(lea.Stages[0].LiquidFuelMass, lea.Stages[0].LiquidFuelTankMass, lea.Stages[0].DeltaV);
                for (var i = 1; i < lea.Stages.Count; i++)
                {
                    var engineDrawer = Instantiate(mEngineDrawer, mEngineDrawer.transform.parent);
                    engineDrawer.Init(lea.Stages[i].EngineCount, lea.Stages[i].Engine);

                    var fuelDrawer = Instantiate(mFuelDrawer, mFuelDrawer.transform.parent);
                    fuelDrawer.Init(lea.Stages[i].LiquidFuelMass, lea.Stages[i].LiquidFuelTankMass, lea.Stages[i].DeltaV);
                    mFuelDrawers.Add(fuelDrawer);
                }

                break;

            case LiquidSolidTwoStageEngineAssembly lsea:
                mEngineDrawer.Init(lsea.Engine1Count, lsea.Engine1);
                var engineDrawer0 = Instantiate(mEngineDrawer, mEngineDrawer.transform.parent);
                engineDrawer0.Init(lsea.Engine0Count, lsea.Engine0, lsea.Engine0Rate);

                mFuelDrawer.Init(lsea.LiquidFuelMass1, lsea.LiquidFuelTankMass1, lsea.DeltaV1);
                var fuelDrawer0 = Instantiate(mFuelDrawer, mFuelDrawer.transform.parent);
                fuelDrawer0.Init(lsea.LiquidFuelMass0, lsea.LiquidFuelTankMass0, lsea.DeltaV0);
                mFuelDrawers.Add(fuelDrawer0);
                break;

            default:
                break;
        }
    }

    public void ShowFuelMass(bool mShowFuelMass)
    {
        foreach (var drawer in mFuelDrawers)
            drawer.ShowFuelMass(mShowFuelMass);
    }

    private void SwitchState()
    {
        mIsSelected = !mIsSelected;
        var colors = mButton.colors;
        colors.normalColor = mIsSelected ? mActive : mNormal;
        mButton.colors = colors;
    }
}
