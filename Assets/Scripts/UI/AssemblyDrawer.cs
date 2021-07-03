using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class AssemblyDrawer : MonoBehaviour
{
    private static NumberFormatInfo mFormat;
   
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

    private static NumberFormatInfo Format
    {
        get
        {
            if (mFormat == null)
            {
                mFormat = new NumberFormatInfo();
                mFormat.NumberDecimalSeparator = ".";
                mFormat.NumberGroupSeparator = " ";
                mFormat.NumberGroupSizes = new int[1] { 3 };
                mFormat.NumberDecimalDigits = 0;
            }
            return mFormat;
        }
    }

    public void Init(LiquidSolidClassicEngineAssembly assembly)
    {
        mEngine0Count.text = assembly.Engine0Count.ToString();
        mEngine0Image.sprite = assembly.Engine0.Sprite;
        mEngine0Alias.text = assembly.Engine0.Alias;
        mEngine0Rate.text = (100f * assembly.Engine0Rate).ToString("N", Format);

        mEngine1Count.text = assembly.Engine1Count.ToString();
        mEngine1Image.sprite = assembly.Engine1.Sprite;
        mEngine1Alias.text = assembly.Engine1.Alias;

        mLiquidFuelTankMass0.text = assembly.LiquidFuelTankMass0.ToString("N", Format) + "kg";
        mLiquidFuelTankMass1.text = assembly.LiquidFuelTankMass1.ToString("N", Format) + "kg";

        mDeltaV.text = assembly.DeltaV.ToString("N", Format) + "m/s";
        mCost.text = assembly.Cost.ToString("N", Format);

        mDeltaV0.text = assembly.DeltaV0.ToString("N", Format) + "m/s";
        mDeltaV1.text = assembly.DeltaV1.ToString("N", Format) + "m/s";

        mTime0.text = assembly.Time0.ToString("N", Format) + "s";
        mTime1.text = assembly.Time1.ToString("N", Format) + "s";
    }
}
