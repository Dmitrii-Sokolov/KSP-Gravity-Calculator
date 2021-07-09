using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VacuumUI : MonoBehaviour
{
    private SortButton mCurrentSort;

    private List<EngineVacuumDrawer> mEngineVacuumDrawers = new List<EngineVacuumDrawer>();

    [SerializeField] private Toggle mAllTechs;
    [SerializeField] private InputField mPayload;

    [SerializeField] private InputField mDeltaV;
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

    public List<Technology> Technologies { get; set; } = new List<Technology>();

    //TODO Major Суммирование нескольких DeltaV + домножение + загрузка в PlayerPrefs

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

        mPayload.onEndEdit.AddListener(_ => Calculate());
        mDeltaV.onEndEdit.AddListener(_ => Calculate());

        if (mAllTechs.isOn)
            Technologies = Technology.GetAll().ToList();

        var engines = Technologies.SelectMany(tech => tech.Parts.OfType<Engine>()).Where(engine => !(engine is SolidFuelEngine));
        foreach (var engine in engines)
        {
            var row = Instantiate(mEngineVacuumDrawer, mRoot);
            row.Init(engine, OnTableChanged);
            mEngineVacuumDrawers.Add(row);
        }

        Calculate();
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

    private void Resort()
    {
        var order = 0;
        var list = mCurrentSort.SortAction(mEngineVacuumDrawers);
        if (!mCurrentSort.Ascending)
            list = list.Reverse();
        foreach (var drawer in list)
            drawer.transform.SetSiblingIndex(order++);
    }

    private void Calculate()
    {
        var payload = float.Parse(mPayload.text);
        var deltaV = float.Parse(mDeltaV.text);

        foreach (var drawer in mEngineVacuumDrawers)
        {
            drawer.Payload = payload;
            drawer.DeltaV = deltaV;
            drawer.Calculate();
        }

        Resort();
        OnTableChanged();
    }
}
