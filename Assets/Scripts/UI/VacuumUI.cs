using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class VacuumUI : MonoBehaviour
{
    [Serializable]
    private class DeltaVPreset
    {
        public string Caption;
        public string DeltaV;
    }
    private SortButton mCurrentSort;

    private readonly List<EngineVacuumDrawer> mEngineVacuumDrawers = new List<EngineVacuumDrawer>();

    [SerializeField] private DeltaVPreset[] mDeltaVPresets;
    [SerializeField] private Button mDeltaVPresetsOpen;
    [SerializeField] private Transform mDeltaVPresetsRoot;
    [SerializeField] private DeltaVPresetDrawer mDeltaVPresetDrawer;

    [Space]
    [SerializeField] private InputField mDeltaVTerms;
    [SerializeField] private InputField mDeltaVMultiplier;
    [SerializeField] private InputField mTargetDeltaV;
    [SerializeField] private InputField mPayload;

    [Space]
    [SerializeField] private Transform mRoot;
    [SerializeField] private EngineVacuumDrawer mEngineVacuumDrawer;

    [Space]
    [SerializeField] private SortButton mEngineMass;
    [SerializeField] private SortButton mMaxDeltaV;
    [SerializeField] private SortButton mImpulseAtmosphere;
    [SerializeField] private SortButton mImpulse;
    [SerializeField] private SortButton mEngineThrust;
    [SerializeField] private SortButton mAlias;
    [SerializeField] private SortButton mThrust;
    [SerializeField] private SortButton mCost;
    [SerializeField] private SortButton mMass;
    [SerializeField] private SortButton mTime;
    [SerializeField] private SortButton mFuelMass;
    [SerializeField] private SortButton mFuelTankMass;

    private float RawDeltaV { get; set; }
    private float DeltaVMultiplier { get; set; }
    private float TargetDeltaV { get; set; }
    private float Payload { get; set; }

    void Start()
    {
        mEngineMass.Init(this, l => l.OrderBy(d => d.EngineMass));
        mMaxDeltaV.Init(this, l => l.OrderBy(d => d.MaxDeltaV));
        mImpulseAtmosphere.Init(this, l => l.OrderBy(d => d.ImpulseAtmosphere));
        mImpulse.Init(this, l => l.OrderBy(d => d.Impulse));
        mEngineThrust.Init(this, l => l.OrderBy(d => d.EngineThrust));
        mAlias.Init(this, l => l.OrderBy(d => d.EngineAlias));
        mThrust.Init(this, l => l.OrderBy(d => d.Thrust));
        mCost.Init(this, l => l.OrderBy(d => d.Cost));
        mMass.Init(this, l => l.OrderBy(d => d.Mass));
        mTime.Init(this, l => l.OrderBy(d => d.Time));
        mFuelMass.Init(this, l => l.OrderBy(d => d.FuelMass));
        mFuelTankMass.Init(this, l => l.OrderBy(d => d.FuelTankMass));

        mCurrentSort = mImpulse;
        mCurrentSort.IsSelected = true;

        mDeltaVTerms.onEndEdit.AddListener(OnDeltaVTermsEndEdit);
        mDeltaVMultiplier.onEndEdit.AddListener(OnDeltaVMultiplayerEndEdit);
        mTargetDeltaV.onEndEdit.AddListener(OnDeltaVEndEdit);
        mPayload.onEndEdit.AddListener(OnPayloadEndEdit);

        TechUI.Instance.OnTechnologiesChanged += Recreate;
        Create();

        Payload = PlayerPrefs.GetInt("Payload", 1000);
        mPayload.text = Payload.ToString("G", Locale.Format);

        DeltaVMultiplier = PlayerPrefs.GetFloat("DeltaVMultiplier", 1.2f);
        mDeltaVMultiplier.text = DeltaVMultiplier.ToString("G", Locale.Format);

        mDeltaVTerms.text = PlayerPrefs.GetString("DeltaVTerms");
        OnDeltaVTermsEndEdit(mDeltaVTerms.text);
        ShowRawDeltaV();

        mDeltaVPresetsOpen.onClick.AddListener(() => mDeltaVPresetsRoot.gameObject.SetActive(!mDeltaVPresetsRoot.gameObject.activeSelf));
        foreach (var preset in mDeltaVPresets)
        {
            var row = Instantiate(mDeltaVPresetDrawer, mDeltaVPresetsRoot);
            row.Init(preset.Caption, preset.DeltaV, OnDeltaVPresetSelected);
        }
    }

    private void OnDeltaVPresetSelected(string deltaV)
    {
        mDeltaVTerms.text = deltaV;
        OnDeltaVTermsEndEdit(mDeltaVTerms.text);
        mDeltaVPresetsRoot.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        TechUI.Instance.OnTechnologiesChanged -= Recreate;
    }

    public void OnTableChanged()
    {
        var cost = mEngineVacuumDrawers.Min(d => d.Cost);
        var mass = mEngineVacuumDrawers.Min(d => d.Mass);

        foreach (var drawer in mEngineVacuumDrawers)
        {
            drawer.BestCost = cost;
            drawer.BestMass = mass;
        }
    }

    public void OnSortButtonClick(SortButton sortButton)
    {
        if (mCurrentSort == sortButton)
        {
            mCurrentSort.Ascending = !mCurrentSort.Ascending;
        }
        else
        {
            mCurrentSort.IsSelected = false;
            mCurrentSort = sortButton;
            mCurrentSort.IsSelected = true;
        }

        Resort();
    }

    private void OnDeltaVTermsEndEdit(string input)
    {
        RawDeltaV = 0f;
        var terms = Regex.Matches(input, "\\d+");
        foreach (Match term in terms)
            RawDeltaV += int.Parse(term.Value, Locale.Format);

        TargetDeltaV = RawDeltaV * DeltaVMultiplier;
        mTargetDeltaV.text = TargetDeltaV.ToString("N", Locale.Format);
        ShowRawDeltaV();
        PlayerPrefs.SetString("DeltaVTerms", input);
        Calculate();
    }

    private void OnDeltaVMultiplayerEndEdit(string input)
    {
        DeltaVMultiplier = float.Parse(input, Locale.Format);
        TargetDeltaV = RawDeltaV * DeltaVMultiplier;
        mTargetDeltaV.text = TargetDeltaV.ToString("N", Locale.Format);
        ShowRawDeltaV();
        PlayerPrefs.SetFloat("DeltaVMultiplier", DeltaVMultiplier);
        Calculate();
    }

    private void OnDeltaVEndEdit(string input)
    {
        TargetDeltaV = int.Parse(input, Locale.Format);
        RawDeltaV = TargetDeltaV / DeltaVMultiplier;
        HideRawDeltaV();
        Calculate();
    }

    private void OnPayloadEndEdit(string input)
    {
        var payload = int.Parse(input, Locale.Format);
        Payload = payload;
        PlayerPrefs.SetInt("Payload", payload);
        Calculate();
    }

    private void Recreate()
    {
        Create();
        Calculate();
    }

    private void Create()
    {
        foreach (var drawer in mEngineVacuumDrawers)
            Destroy(drawer.gameObject);
        mEngineVacuumDrawers.Clear();

        var engines = TechUI.Instance.Technologies.SelectMany(tech => tech.Parts.OfType<Engine>()).Where(engine => !(engine is SolidFuelEngine));
        foreach (var engine in engines)
        {
            var row = Instantiate(mEngineVacuumDrawer, mRoot);
            row.Init(engine, OnTableChanged);
            mEngineVacuumDrawers.Add(row);
        }
    }

    private void Resort()
    {
        if (mEngineVacuumDrawers.Count == 0)
            return;

        var order = 0;
        var list = mCurrentSort.SortAction(mEngineVacuumDrawers);
        if (!mCurrentSort.Ascending)
            list = list.Reverse();
        foreach (var drawer in list)
            drawer.transform.SetSiblingIndex(order++);
    }

    private void Calculate()
    {
        if (mEngineVacuumDrawers.Count == 0)
            return;

        foreach (var drawer in mEngineVacuumDrawers)
        {
            drawer.DeltaV = TargetDeltaV;
            drawer.Payload = Payload;
            drawer.Calculate();
        }

        Resort();
        OnTableChanged();
    }

    private void ShowRawDeltaV()
    {
        mDeltaVTerms.targetGraphic.color = Color.white;
        mDeltaVMultiplier.targetGraphic.color = Color.white;
    }

    private void HideRawDeltaV()
    {
        mDeltaVTerms.targetGraphic.color = Color.gray;
        mDeltaVMultiplier.targetGraphic.color = Color.gray;
    }
}
