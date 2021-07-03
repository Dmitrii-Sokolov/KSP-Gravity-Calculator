using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    private AssembliesIterator mAssembliesIterator = new AssembliesIterator();
    private List<AssemblyDrawer> mAssemblyDrawers = new List<AssemblyDrawer>();

    [SerializeField] private Toggle mRandomOrder;
    [SerializeField] private Toggle mAllTechs;
    [SerializeField] private InputField mPayload;
    [SerializeField] private Button mStart;
    [SerializeField] private Button mStop;

    [Space]
    [SerializeField] private Transform mTableRoot;
    [SerializeField] private AssemblyDrawer mAssemblyDrawerPrefab;

    [Space]
    [SerializeField] private ScriptableObject[] mScriptableObjects;

    void Start()
    {
        mRandomOrder.isOn = mAssembliesIterator.UseRandomization;
        mAllTechs.isOn = mAssembliesIterator.UseAllTechnologies;
        mPayload.text = mAssembliesIterator.Payload.ToString();

        mRandomOrder.onValueChanged.AddListener(value => mAssembliesIterator.UseRandomization = value);
        mAllTechs.onValueChanged.AddListener(value => mAssembliesIterator.UseAllTechnologies = value);
        mPayload.onValueChanged.AddListener(value => mAssembliesIterator.Payload = float.Parse(value));
        mStart.onClick.AddListener(mAssembliesIterator.StartIterating);
        mStop.onClick.AddListener(mAssembliesIterator.StopIterating);
    }

    void Update()
    {
        foreach (var drawer in mAssemblyDrawers)
            Destroy(drawer);
        mAssemblyDrawers.Clear();

        foreach (var assembly in mAssembliesIterator.Assemblies.OrderBy(assembly => assembly.Cost))
        {
            var row = Instantiate(mAssemblyDrawerPrefab, mTableRoot);
            row.Init(assembly);
            mAssemblyDrawers.Add(row);
        }
    }
}
