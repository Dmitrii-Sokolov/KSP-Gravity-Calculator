using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Graphic))]
public class SortButton : MonoBehaviour
{
    private bool mIsSelected;

    private Graphic mGraphic;

    [SerializeField] private bool mAscending;
    [SerializeField] private Color mNormalColor;
    [SerializeField] private Color mAscendingColor;
    [SerializeField] private Color mDescendingColor;

    public bool Ascending
    {
        get
        {
            return mAscending;
        }
        set
        {
            mAscending = value;
            CheckColor();
        }
    }

    public bool IsSelected
    {
        get
        {
            return mIsSelected;
        }
        set
        {
            mIsSelected = value;
            CheckColor();
        }
    }

    public Func<IEnumerable<EngineVacuumDrawer>, IEnumerable<EngineVacuumDrawer>> SortAction { get; private set; }

    public void Init(VacuumUI vacuumUI, Func<IEnumerable<EngineVacuumDrawer>, IEnumerable<EngineVacuumDrawer>> sortAction)
    {
        SortAction = sortAction;
        GetComponent<Button>().onClick.AddListener(() => vacuumUI.OnSortButtonClick(this));
        mGraphic = GetComponent<Graphic>();
        CheckColor();
    }

    private void CheckColor()
    {
        mGraphic.color = !IsSelected
            ? mNormalColor
            : Ascending
                ? mAscendingColor
                : mDescendingColor;
    }
}
