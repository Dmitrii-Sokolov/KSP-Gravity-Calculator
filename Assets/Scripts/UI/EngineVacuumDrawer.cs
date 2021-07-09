using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EngineVacuumDrawer : MonoBehaviour
{
    private Decoupler mDecoupler;
    private Engine mEngine;
    private float mMaxDeltaV;
    private int mEnginesCountMin;
    private int mStagesCountMin;

    private float mDeltaV;
    private float mPayload;
    private int mEnginesCount;
    private int mStagesCount;

    public (float FuelMass, float TankMass, float DeltaV)[] Stages;

    private bool mIsSelected = false;

    [SerializeField] private Color mNormal;
    [SerializeField] private Color mActive;

    [SerializeField] private Text mEngineMassText;
    [SerializeField] private Text mMaxDeltaVText;
    [SerializeField] private Text mImpulseAtmosphereText;
    [SerializeField] private Text mImpulseText;
    [SerializeField] private Text mEngineThrustText;
    [SerializeField] private Image mEngineImage;
    [SerializeField] private Text mEngineAlias;

    [SerializeField] private Text mEnginesCountText;
    [SerializeField] private Button mEnginesCountMinus;
    [SerializeField] private Button mEnginesCountPlus;
    [SerializeField] private Text mStagesCountText;
    [SerializeField] private Button mStagesCountMinus;
    [SerializeField] private Button mStagesCountPlus;

    [SerializeField] private Text mThrustText;

    [SerializeField] private Graphic mCostRating;
    [SerializeField] private Graphic mMassRating;

    [SerializeField] private Text mCostText;
    [SerializeField] private Text mMassText;
    [SerializeField] private Text mTimeText;
    [SerializeField] private Text mFuelMassText;
    [SerializeField] private Text mFuelTankMassText;

    [SerializeField] private List<FuelDrawer> mFuelDrawers;

    [SerializeField] private Button mButton;

    public float EngineMass => mEngine.Mass;
    public float MaxDeltaV => mMaxDeltaV;
    public float ImpulseAtmosphere => mEngine.ImpulseAtOneAtmosphere;
    public float Impulse => mEngine.Impulse;
    public float EngineThrust => mEngine.Thrust;
    public string EngineAlias => mEngine.Alias;
    public float Thrust => EnginesCount * mEngine.Thrust;

    public float Time { get; private set; }
    public float Cost { get; private set; }
    public float Mass { get; private set; }
    public float FuelMass { get; private set; }
    public float FuelTankMass { get; private set; }

    public float Payload
    {
        get
        {
            return mPayload;
        }
        set
        {
            mPayload = value;
        }
    }

    public float DeltaV
    {
        get
        {
            return mDeltaV;
        }
        set
        {
            mDeltaV = value;
            //Считаем, что на одну ступень не стоит нагружать более половины критической DeltaV
            //Разница между ступенью DeltaV / 2 и двумя ступенями по DeltaV / 4 составит 13% по массе топлива
            //На практике, число ступеней стоит увеличивать до предела опираясь на максимально доступную сложность сборки и желаемую комфортностью управления
            mStagesCountMin = Mathf.CeilToInt(2f * DeltaV / mMaxDeltaV);
            StagesCount = Mathf.Max(StagesCount, mStagesCountMin);
        }
    }

    public int EnginesCount
    {
        get
        {
            return mEnginesCount;
        }
        private set
        {
            mEnginesCount = value;
            mEnginesCountText.text = value.ToString("N", Locale.Format);
            mThrustText.text = Thrust.ToString("N", Locale.Format) + "kN";
            mEnginesCountMinus.interactable = mEnginesCount > mEnginesCountMin;
        }
    }

    public int StagesCount
    {
        get
        {
            return mStagesCount;
        }
        private set
        {
            mStagesCount = value;
            mStagesCountText.text = value.ToString("N", Locale.Format);
            mStagesCountMinus.interactable = mStagesCount > mStagesCountMin;
        }
    }

    public float BestCost
    {
        set
        {
            var color = mCostRating.color;
            color.a = GetRatingAlpha(value / Cost);
            mCostRating.color = color;
        }
    }

    public float BestMass
    {
        set
        {
            var color = mMassRating.color;
            color.a = GetRatingAlpha(value / Mass);
            mMassRating.color = color;
        }
    }

    public void Init(Engine engine, Action onValuesChanged)
    {
        mDecoupler = Part.GetAll<Decoupler>().FirstOrDefault(p => p.Alias == "TD-12");
        mEngine = engine;
        mMaxDeltaV = engine.MaxDeltaV;

        mEngineMassText.text = engine.Mass.ToString("N", Locale.Format) + "kg";
        mMaxDeltaVText.text = mMaxDeltaV.ToString("N", Locale.Format) + "m/s";
        mImpulseAtmosphereText.text = engine.ImpulseAtOneAtmosphere.ToString("N", Locale.Format) + "s";
        mImpulseText.text = engine.Impulse.ToString("N", Locale.Format) + "s";
        mEngineThrustText.text = engine.Thrust.ToString("N", Locale.Format) + "kN";
        mEngineImage.sprite = engine.Sprite;
        mEngineAlias.text = engine.Alias;

        mEnginesCountMin = mEngine.RadialMountedOnly ? 2 : 1;
        EnginesCount = mEnginesCountMin;

        mStagesCountMinus.onClick.AddListener(() => { StagesCount--; Calculate(); onValuesChanged?.Invoke(); });
        mStagesCountPlus.onClick.AddListener(() => { StagesCount++; Calculate(); onValuesChanged?.Invoke(); });
        mEnginesCountMinus.onClick.AddListener(() => { EnginesCount--; Calculate(); onValuesChanged?.Invoke(); });
        mEnginesCountPlus.onClick.AddListener(() => { EnginesCount++; Calculate(); onValuesChanged?.Invoke(); });
        mButton.onClick.AddListener(SwitchState);
    }

    public void Calculate()
    {
        Stages = new (float FuelMass, float TankMass, float DeltaV)[StagesCount];

        Time = 0f;
        Cost = EnginesCount * mEngine.Cost;
        Mass = Payload + EnginesCount * mEngine.Mass;
        FuelMass = 0f;
        FuelTankMass = 0f;

        var stageDeltaV = DeltaV / StagesCount;
        var tank = Constants.FuelMassToTankMass(mEngine.Fuel);
        var exp = Mathf.Exp(stageDeltaV / (mEngine.Impulse * Constants.g));
        var multiplier = (exp - 1f) / (tank + exp - tank * exp);
        if (multiplier < 0f)
            return;

        for (var stage = 0; stage < StagesCount; stage++)
        {
            var (fuelMass, fuelTankMass, fuelCost) = Constants.SplitFuel(Mass * multiplier, mEngine.Fuel);
            var deltaV = Impulse * Constants.g * Mathf.Log((Mass + fuelTankMass) / (Mass + fuelTankMass - fuelMass));
            Cost += fuelCost + (stage == 0 ? 0f : mDecoupler.Cost);
            Mass += fuelTankMass + (stage == 0 ? 0f : mDecoupler.Mass);
            Time += fuelMass / (EnginesCount * mEngine.FuelConsumption);
            FuelMass += fuelMass;
            FuelTankMass += fuelTankMass;
            Stages[stage] = (fuelMass, fuelTankMass, deltaV);
        }

        mCostText.text = Cost.ToString("N", Locale.Format);
        mMassText.text = Mass.ToString("N", Locale.Format) + "kg";
        mTimeText.text = Time.ToString("N", Locale.Format) + "s";
        mFuelMassText.text = FuelMass.ToString("N", Locale.Format) + "kg";
        mFuelTankMassText.text = FuelTankMass.ToString("N", Locale.Format) + "kg";

        for (var i = mFuelDrawers.Count; i < Stages.Length; i++)
            mFuelDrawers.Add(Instantiate(mFuelDrawers[0], mFuelDrawers[0].transform.parent));

        for (var i = 0; i < mFuelDrawers.Count; i++)
            mFuelDrawers[i].gameObject.SetActive(i < Stages.Length);

        for (var i = 0; i < Stages.Length; i++)
            mFuelDrawers[i].Init(Stages[i].FuelMass, Stages[i].TankMass, Stages[i].DeltaV);
    }

    private void SwitchState()
    {
        mIsSelected = !mIsSelected;
        var colors = mButton.colors;
        colors.normalColor = mIsSelected ? mActive : mNormal;
        mButton.colors = colors;
        mFuelDrawers[0].transform.parent.gameObject.SetActive(mIsSelected);
    }

    private float GetRatingAlpha(float value)
    {
        return Mathf.Clamp01(value);
    }
}
