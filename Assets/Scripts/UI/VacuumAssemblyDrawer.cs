using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VacuumAssemblyDrawer : MonoBehaviour
{
    private List<FuelDrawer> mFuelDrawers;

    private bool mIsSelected = false;

    [SerializeField] private Color mNormal;
    [SerializeField] private Color mActive;

    [SerializeField] private Text mEngineCount;
    [SerializeField] private Image mEngineImage;
    [SerializeField] private Text mEngineAlias;
    [SerializeField] private Text mStagesCount;

    [SerializeField] private Text mMass;
    [SerializeField] private Text mCost;
    [SerializeField] private Text mTime;

    [SerializeField] private FuelDrawer mFuelDrawer;

    [SerializeField] private Button mButton;

    public void Init(VacuumAssembly assembly)
    {
        mFuelDrawers = new List<FuelDrawer>() { mFuelDrawer };
        mButton.onClick.AddListener(SwitchState);

        mEngineCount.text = assembly.EnginesCount.ToString();
        mEngineImage.sprite = assembly.Engine.Sprite;
        mEngineAlias.text = assembly.Engine.Alias;
        mStagesCount.text = assembly.StagesCount.ToString("N", Locale.Format);

        mMass.text = assembly.Mass.ToString("N", Locale.Format) + "kg";
        mCost.text = assembly.Cost.ToString("N", Locale.Format);
        mTime.text = assembly.Time.ToString("N", Locale.Format) + "s";

        mFuelDrawer.Init(assembly.Stages[0].FuelMass, assembly.Stages[0].TankMass, assembly.Stages[0].DeltaV);
        for (var i = 1; i < assembly.Stages.Length; i++)
        {
            var fuelDrawer = Instantiate(mFuelDrawer, mFuelDrawer.transform.parent);
            fuelDrawer.Init(assembly.Stages[i].FuelMass, assembly.Stages[i].TankMass, assembly.Stages[i].DeltaV);
            mFuelDrawers.Add(fuelDrawer);
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
        mFuelDrawer.transform.parent.gameObject.SetActive(mIsSelected);
    }
}
