using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VacuumUI : MonoBehaviour
{
    private List<EngineVacuumDrawer> mEngineVacuumDrawers = new List<EngineVacuumDrawer>();

    [SerializeField] private Toggle mAllTechs;
    [SerializeField] private InputField mPayload;
    [SerializeField] private InputField mDeltaV;
    [SerializeField] private Transform mRoot;
    [SerializeField] private EngineVacuumDrawer mEngineVacuumDrawer;

    [Space]
    [SerializeField] private Button mEngineMass;
    [SerializeField] private Button mMaxDeltaV;
    [SerializeField] private Button mImpulse;
    [SerializeField] private Button mEngineThrust;
    [SerializeField] private Button mAlias;
    [SerializeField] private Button mThrust;
    [SerializeField] private Button mCost;
    [SerializeField] private Button mMass;
    [SerializeField] private Button mTime;
    [SerializeField] private Button mFuelMass;
    [SerializeField] private Button mFuelTankMass;


    public List<Technology> Technologies { get; set; } = new List<Technology>();

    //TODO Major Суммирование нескольких DeltaV + домножение + загрузка в PlayerPrefs
    //TODO Major Подсветка оптимальных

    void Start()
    {
        mEngineMass.onClick.AddListener(() => Sort(d => d.EngineMass));
        mMaxDeltaV.onClick.AddListener(() => Sort(d => d.MaxDeltaV));
        mImpulse.onClick.AddListener(() => Sort(d => d.Impulse));
        mEngineThrust.onClick.AddListener(() => Sort(d => d.EngineThrust));
        mAlias.onClick.AddListener(() => Sort(d => d.EngineAlias));
        mThrust.onClick.AddListener(() => Sort(d => d.Thrust));
        mCost.onClick.AddListener(() => Sort(d => d.Cost));
        mMass.onClick.AddListener(() => Sort(d => d.Mass));
        mTime.onClick.AddListener(() => Sort(d => d.Time));
        mFuelMass.onClick.AddListener(() => Sort(d => d.FuelMass));
        mFuelTankMass.onClick.AddListener(() => Sort(d => d.FuelTankMass));

        mPayload.onEndEdit.AddListener(_ => Calculate());
        mDeltaV.onEndEdit.AddListener(_ => Calculate());

        if (mAllTechs.isOn)
            Technologies = Technology.GetAll().ToList();

        var engines = Technologies.SelectMany(tech => tech.Parts.OfType<Engine>()).Where(engine => !(engine is SolidFuelEngine));
        foreach (var engine in engines)
        {
            var row = Instantiate(mEngineVacuumDrawer, mRoot);
            row.Init(engine);
            mEngineVacuumDrawers.Add(row);
        }

        Calculate();
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
    }

    private void Sort<T>(Func<EngineVacuumDrawer, T> sorter)
    {
        var order = 0;
        foreach (var drawer in mEngineVacuumDrawers.OrderBy(sorter))
            drawer.transform.SetSiblingIndex(order++);
    }
}
