using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    private bool mShowFuelMass = false;
    private LiquidIterator mLiquidIterator = new LiquidIterator();
    private LiquidSolidIterator mLiquidSolidIterator = new LiquidSolidIterator();
    private List<AssemblyDrawer> mAssemblyDrawers = new List<AssemblyDrawer>();

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
    [SerializeField] private ScriptableObject[] mScriptableObjects;

    //TODO UI для рассчётов в вакууме
    //TODO UI для суммирования
    //TODO UI для таблицы с двигателями + фильтр + сортировка
    //TODO UI для таблицы с топливными баками

    void Start()
    {
        var test = new VacuumIterator();
        test.Calculate();

        mPayload.onEndEdit.AddListener(_ => Calculate());
        mSingleStage.onValueChanged.AddListener(_ => Calculate());
        mSwitchFuel.onClick.AddListener(SwitchFuel);
        SwitchFuel();
    }

    private void SwitchFuel()
    {
        mShowFuelMass = !mShowFuelMass;
        mSwitchFuelText.text = mShowFuelMass ? "Fuel Mass" : "Fuel Tank Mass";
        foreach (var drawer in mAssemblyDrawers)
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
}
