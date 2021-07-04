using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    private IteratorBase mAssembliesIterator = new LiquidIterator();
    private List<AssemblyDrawer> mAssemblyDrawers = new List<AssemblyDrawer>();

    [SerializeField] private Toggle mAllTechs;
    [SerializeField] private InputField mPayload;
    [SerializeField] private Button mCalculate;
    [SerializeField] private Button mIteratorChange;
    [SerializeField] private Text mCurrentIteratorCaption;

    [Space]
    [SerializeField] private Transform mTableRoot;
    [SerializeField] private AssemblyDrawer mAssemblyDrawerPrefab;

    [Space]
    [SerializeField] private ScriptableObject[] mScriptableObjects;

    void Start()
    {
        mCurrentIteratorCaption.text = mAssembliesIterator.GetType().ToString();
        mCalculate.onClick.AddListener(Calculate);
        mIteratorChange.onClick.AddListener(ChangeIterator);
    }

    void ChangeIterator()
    {
        mAssembliesIterator = mAssembliesIterator is LiquidIterator
            ? new LiquidSolidIterator()
            : (IteratorBase)new LiquidIterator();
        mCurrentIteratorCaption.text = mAssembliesIterator.GetType().ToString();
    }

    void Calculate()
    {
        mAssembliesIterator.UseAllTechnologies = mAllTechs.isOn;
        mAssembliesIterator.Payload = float.Parse(mPayload.text);

        foreach (var drawer in mAssemblyDrawers)
            Destroy(drawer.gameObject);
        mAssemblyDrawers.Clear();

        mAssembliesIterator.Calculate();

        foreach (var assembly in mAssembliesIterator.Assemblies)
        {
            var row = Instantiate(mAssemblyDrawerPrefab, mTableRoot);
            row.Init(assembly);
            mAssemblyDrawers.Add(row);
        }
    }
}
