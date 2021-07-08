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

    public List<Technology> Technologies { get; set; } = new List<Technology>();

    //TODO Major Суммирование нескольких DeltaV + домножение + загрузка в PlayerPrefs
    //TODO Major Сортировка
    //TODO Major Подсветка оптимальных

    void Start()
    {
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
}
