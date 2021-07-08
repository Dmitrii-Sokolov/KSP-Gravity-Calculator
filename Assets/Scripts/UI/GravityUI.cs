using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GravityUI : MonoBehaviour
{
    private LiquidIterator mLiquidIterator = new LiquidIterator();
    private LiquidSolidIterator mLiquidSolidIterator = new LiquidSolidIterator();

    private List<AssemblyDrawer> mAssemblyDrawers = new List<AssemblyDrawer>();

    [SerializeField] private Toggle mAllTechs;
    [SerializeField] private Toggle mSingleStage;
    [SerializeField] private InputField mPayload;
    [SerializeField] private Transform mLiquidRoot;
    [SerializeField] private Transform mSolidRoot;
    [SerializeField] private AssemblyDrawer mAssemblyDrawerPrefab;

    void Start()
    {
        mPayload.onEndEdit.AddListener(_ => Calculate());
        mSingleStage.onValueChanged.AddListener(_ => Calculate());
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
            mAssemblyDrawers.Add(row);
        }

        foreach (var assembly in mLiquidSolidIterator.Assemblies)
        {
            var row = Instantiate(mAssemblyDrawerPrefab, mSolidRoot);
            row.Init(assembly);
            mAssemblyDrawers.Add(row);
        }
    }
}
