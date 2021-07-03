using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    private LiquidSolidIterator mAssembliesIterator = new LiquidSolidIterator();
    private List<AssemblyDrawer> mAssemblyDrawers = new List<AssemblyDrawer>();

    [SerializeField] private Toggle mAllTechs;
    [SerializeField] private InputField mPayload;
    [SerializeField] private Button mCalculate;

    [Space]
    [SerializeField] private Transform mTableRoot;
    [SerializeField] private AssemblyDrawer mAssemblyDrawerPrefab;

    [Space]
    [SerializeField] private ScriptableObject[] mScriptableObjects;

    void Start()
    {
        mAllTechs.isOn = mAssembliesIterator.UseAllTechnologies;
        mPayload.text = mAssembliesIterator.Payload.ToString();

        mAllTechs.onValueChanged.AddListener(value => mAssembliesIterator.UseAllTechnologies = value);
        mPayload.onValueChanged.AddListener(value => mAssembliesIterator.Payload = float.Parse(value));
        mCalculate.onClick.AddListener(Calculate);
    }

    void Calculate()
    {
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
