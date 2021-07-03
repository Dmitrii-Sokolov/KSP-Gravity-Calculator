using UnityEngine;
using UnityEngine.UI;

public class AssemblyDrawer : MonoBehaviour
{
    [SerializeField] private Text mEngine0Count;
    [SerializeField] private Image mEngine0Image;
    [SerializeField] private Text mEngine0Alias;
    [SerializeField] private Text mEngine0Rate;

    [SerializeField] private Text mEngine1Count;
    [SerializeField] private Image mEngine1Image;
    [SerializeField] private Text mEngine1Alias;

    [SerializeField] private Text mLiquidFuelTankMass0;
    [SerializeField] private Text mLiquidFuelTankMass1;

    [SerializeField] private Text mDeltaV;
    [SerializeField] private Text mCost;

    [SerializeField] private Text mDeltaV0;
    [SerializeField] private Text mDeltaV1;

    [SerializeField] private Text mTime0;
    [SerializeField] private Text mTime1;

    public float Cost { get; private set; }

    public void Init(LiquidSolidClassicEngineAssembly assembly)
    {
        //Сделать нормально + группировка по три цифры
        //Моноширинный шрифт

        mEngine0Count.text = assembly.Engine0Count.ToString();
        mEngine0Image.sprite = assembly.Engine0.Sprite;
        mEngine0Alias.text = assembly.Engine0.Alias;
        mEngine0Rate.text = assembly.Engine0Rate.ToString("0.00");

        mEngine1Count.text = assembly.Engine1Count.ToString();
        mEngine1Image.sprite = assembly.Engine1.Sprite;
        mEngine1Alias.text = assembly.Engine1.Alias;

        mLiquidFuelTankMass0.text = assembly.LiquidFuelTankMass0.ToString("0.0") + "kg";
        mLiquidFuelTankMass1.text = assembly.LiquidFuelTankMass1.ToString("0.0") + "kg";

        mDeltaV.text = assembly.DeltaV.ToString() + "m/s";
        mCost.text = assembly.Cost.ToString();

        mDeltaV0.text = assembly.DeltaV0.ToString() + "m/s";
        mDeltaV1.text = assembly.DeltaV1.ToString() + "m/s";

        mTime0.text = assembly.Time0.ToString() + "s";
        mTime1.text = assembly.Time0.ToString() + "s";

        Cost = assembly.Cost;
    }
}
