using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    private bool mShowFuelMass = false;

    private LiquidIterator mLiquidIterator = new LiquidIterator();
    private LiquidSolidIterator mLiquidSolidIterator = new LiquidSolidIterator();
    private VacuumIterator mVacuumIterator = new VacuumIterator();

    private List<AssemblyDrawer> mAssemblyDrawers = new List<AssemblyDrawer>();
    private List<VacuumAssemblyDrawer> mVacuumAssemblyDrawers = new List<VacuumAssemblyDrawer>();

    [SerializeField] private Toggle mAllTechs;
    [SerializeField] private Toggle mSingleStage;
    [SerializeField] private Button mSwitchFuel;
    [SerializeField] private Text mSwitchFuelText;
    [SerializeField] private InputField mPayload;

    [Space]
    [SerializeField] private Transform mLiquidRoot;
    [SerializeField] private Transform mSolidRoot;
    [SerializeField] private AssemblyDrawer mAssemblyDrawerPrefab;

    [Space]
    [SerializeField] private InputField mPayloadVacuum;
    [SerializeField] private InputField mDeltaV;
    [SerializeField] private Transform mVacuumRoot;
    [SerializeField] private VacuumAssemblyDrawer mVacuumAssemblyDrawer;

    [Space]
    [SerializeField] private ScriptableObject[] mScriptableObjects;

    //TODO Medium UI для суммирования DeltaV в вакууме

    //TODO Minor UI для таблицы с двигателями + фильтр + сортировка
    //TODO Minor UI для таблицы с топливными баками

    void Start()
    {
        mPayload.onEndEdit.AddListener(_ => Calculate());
        mSingleStage.onValueChanged.AddListener(_ => Calculate());

        mPayloadVacuum.onEndEdit.AddListener(_ => CalculateVacuum());
        mDeltaV.onEndEdit.AddListener(_ => CalculateVacuum());

        mSwitchFuel.onClick.AddListener(SwitchFuel);
        SwitchFuel();
    }

    private void SwitchFuel()
    {
        mShowFuelMass = !mShowFuelMass;
        mSwitchFuelText.text = mShowFuelMass ? "Fuel Mass" : "Fuel Tank Mass";
        foreach (var drawer in mAssemblyDrawers)
            drawer.ShowFuelMass(mShowFuelMass);
        foreach (var drawer in mVacuumAssemblyDrawers)
            drawer.ShowFuelMass(mShowFuelMass);
    }
    
    private void Calculate()
    {
        foreach (var drawer in mAssemblyDrawers)
            Destroy(drawer.gameObject);
        mAssemblyDrawers.Clear();

        mLiquidIterator.UseAllTechnologies = mLiquidSolidIterator.UseAllTechnologies = mAllTechs.isOn;
        mLiquidIterator.Payload = mLiquidSolidIterator.Payload = float.Parse(mPayload.text);
        mLiquidIterator.UseOneStage = mSingleStage.isOn;

        mLiquidIterator.Calculate();
        mLiquidSolidIterator.Calculate();

        foreach (var assembly in mLiquidIterator.Assemblies)
        {
            var row = Instantiate(mAssemblyDrawerPrefab, mLiquidRoot);
            row.Init(assembly);
            row.ShowFuelMass(mShowFuelMass);
            mAssemblyDrawers.Add(row);
        }

        foreach (var assembly in mLiquidSolidIterator.Assemblies)
        {
            var row = Instantiate(mAssemblyDrawerPrefab, mSolidRoot);
            row.Init(assembly);
            row.ShowFuelMass(mShowFuelMass);
            mAssemblyDrawers.Add(row);
        }
    }

    private void CalculateVacuum()
    {
        foreach (var drawer in mVacuumAssemblyDrawers)
            Destroy(drawer.gameObject);
        mVacuumAssemblyDrawers.Clear();

        mVacuumIterator.UseAllTechnologies = mAllTechs.isOn;
        mVacuumIterator.Payload = float.Parse(mPayloadVacuum.text);
        mVacuumIterator.DeltaV = float.Parse(mDeltaV.text);
        mVacuumIterator.Calculate();

        foreach (var assembly in mVacuumIterator.Assemblies)
        {
            var row = Instantiate(mVacuumAssemblyDrawer, mVacuumRoot);
            row.Init(assembly);
            row.ShowFuelMass(mShowFuelMass);
            mVacuumAssemblyDrawers.Add(row);
        }
    }
}
