using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TechUI : MonoBehaviour
{
    private List<TechDrawer> mTechDrawers = new List<TechDrawer>();
    private List<Technology> mTechnologies = new List<Technology>();
    private List<Technology> mTechnologiesBackUp = new List<Technology>();
    private bool mAllTechnologiesBackUp = false;

    [SerializeField] private Toggle mAllTechs;

    [SerializeField] private Button mOpen;
    [SerializeField] private Button mApply;
    [SerializeField] private Button mCancel;

    [SerializeField] private GameObject mCanvas;
    [SerializeField] private Transform mRoot;
    [SerializeField] private TechDrawer mTechDrawer;

    public List<Technology> Technologies
    {
        get
        {
            return mAllTechs.isOn
                ? Technology.GetAll().ToList()
                : mTechnologies;
        }
        private set
        {
            mAllTechs.isOn = false;
            mTechnologies = value;
            OnTechnologiesChanged?.Invoke();
        }
    }

    //TODO Minor Filtering by engine

    public event Action OnTechnologiesChanged;

    public static TechUI Instance { get; private set; }

    private void Awake()
    {
        Instance = Instance == null ? this : throw new Exception("TechUI");
    }

    void Start()
    {
        foreach (var tech in Technology.GetAll().OrderBy(t => t.Level).ThenBy(t => t.Name))
        {
            var row = Instantiate(mTechDrawer, mRoot);
            row.Init(tech, this);
            mTechDrawers.Add(row);
        }

        LoadFromPlayerPrefs();

        mOpen.onClick.AddListener(Open);
        mApply.onClick.AddListener(Apply);
        mCancel.onClick.AddListener(Cancel);
        mAllTechs.onValueChanged.AddListener(_ => { OnTechnologiesChanged?.Invoke(); SaveToPlayerPrefs(); });
    }

    public void TechListChangedByTech()
    {
        Technologies = mTechDrawers.Where(d => d.IsSelected).Select(d => d.Tech).ToList();
    }

    private void Open()
    {
        mCanvas.SetActive(true);
        mTechnologiesBackUp = mTechnologies.ToList();
        mAllTechnologiesBackUp = mAllTechs.isOn;
    }

    private void Apply()
    {
        mCanvas.SetActive(false);
        mTechnologiesBackUp = null;
        SaveToPlayerPrefs();
    }

    private void Cancel()
    {
        mCanvas.SetActive(false);
        Technologies = mTechnologiesBackUp;
        mAllTechs.isOn = mAllTechnologiesBackUp;
        mTechnologiesBackUp = null;
        foreach (var drawer in mTechDrawers)
            drawer.IsSelected = mTechnologies.Contains(drawer.Tech);
    }

    private void SaveToPlayerPrefs()
    {
        var storage = SetBit(mTechDrawers.Count, mAllTechs.isOn);
        for (var i = 0; i < mTechDrawers.Count; i++)
            storage = SetBit(storage, i, mTechDrawers[i].IsSelected);

        PlayerPrefs.SetInt("Technologies", storage);
    }

    private void LoadFromPlayerPrefs()
    {
        var storage = PlayerPrefs.GetInt("Technologies");

        for (var i = 0; i < mTechDrawers.Count; i++)
            mTechDrawers[i].IsSelected = GetBit(storage, i);

        TechListChangedByTech();
        mAllTechs.isOn = GetBit(storage, mTechDrawers.Count);
    }

    private static int SetBit(int index, bool value)
    {
        return SetBit(0, index, value);
    }

    private static int SetBit(int storage, int index, bool value)
    {
        return value ? storage | 1 << index : storage;
    }

    private static bool GetBit(int storage, int index)
    {
        return (storage & (1 << index)) != 0;
    }
}
