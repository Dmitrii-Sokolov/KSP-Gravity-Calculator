using UnityEngine;
using UnityEngine.UI;

public class TechDrawer : MonoBehaviour
{
    private bool mIsSelected;
    private TechUI mTechUI;

    [SerializeField] private Color mNormal;
    [SerializeField] private Color mActive;
    [SerializeField] private Button mButton;
    [SerializeField] private Text mLevel;
    [SerializeField] private Image mImage;
    [SerializeField] private Text mName;

    public bool IsSelected
    {
        get
        {
            return mIsSelected;
        }
        set
        {
            mIsSelected = value;
            var colors = mButton.colors;
            colors.normalColor = IsSelected ? mActive : mNormal;
            mButton.colors = colors;
        }
    }

    public Technology Tech { get; private set; }

    public void Init(Technology tech, TechUI techUI)
    {
        Tech = tech;
        mTechUI = techUI;
        mLevel.text = tech.Level.ToString("N", Locale.Format);
        mImage.sprite = tech.Sprite;
        mImage.color = tech.Sprite == null ? Color.clear : Color.white;
        mName.text = tech.Name;
        mButton.onClick.AddListener(SwitchState);
    }

    private void SwitchState()
    {
        IsSelected = !IsSelected;
        mTechUI.TechListChangedByTech();
    }
}
