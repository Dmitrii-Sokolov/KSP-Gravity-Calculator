using UnityEngine;
using UnityEngine.UI;

public class AssemblyDrawer : MonoBehaviour
{
    [SerializeField] private EngineDrawer mEngineDrawer;
    [SerializeField] private Text mDeltaV;
    [SerializeField] private Text mCost;
    [SerializeField] private FuelDrawer mFuelDrawer;

    public void Init(IEngineAssembly assembly)
    {
        mDeltaV.text = assembly.DeltaV.ToString("N", Locale.Format) + "m/s";
        mCost.text = assembly.Cost.ToString("N", Locale.Format);

        switch (assembly)
        {
            case LiquidEngineAssembly lea:
                mEngineDrawer.Init(lea.Stages[0].EngineCount, lea.Stages[0].Engine);
                for (var i = 1; i < lea.Stages.Count; i++)
                {
                    var engineDrawer = Instantiate(mEngineDrawer, mEngineDrawer.transform.parent);
                    engineDrawer.transform.SetAsFirstSibling();
                    engineDrawer.Init(lea.Stages[i].EngineCount, lea.Stages[i].Engine);
                }

                mFuelDrawer.Init(lea.Stages[lea.Stages.Count - 1].LiquidFuelTankMass, lea.Stages[lea.Stages.Count - 1].DeltaV);
                for (var i = lea.Stages.Count - 2; i >= 0; i--)
                {
                    var fuelDrawer = Instantiate(mFuelDrawer, mEngineDrawer.transform.parent);
                    fuelDrawer.transform.SetAsLastSibling();
                    fuelDrawer.Init(lea.Stages[i].LiquidFuelTankMass, lea.Stages[i].DeltaV);
                }

                break;

            case LiquidSolidTwoStageEngineAssembly lsea:
                var engineDrawer0 = Instantiate(mEngineDrawer, mEngineDrawer.transform.parent);
                engineDrawer0.transform.SetAsFirstSibling();
                engineDrawer0.Init(lsea.Engine0Count, lsea.Engine0, lsea.Engine0Rate);
                mEngineDrawer.Init(lsea.Engine1Count, lsea.Engine1);

                mFuelDrawer.Init(lsea.LiquidFuelTankMass0, lsea.DeltaV0);
                var fuelDrawer1 = Instantiate(mFuelDrawer, mEngineDrawer.transform.parent);
                fuelDrawer1.transform.SetAsLastSibling();
                fuelDrawer1.Init(lsea.LiquidFuelTankMass1, lsea.DeltaV1);
                break;

            default:
                break;
        }
    }
}
