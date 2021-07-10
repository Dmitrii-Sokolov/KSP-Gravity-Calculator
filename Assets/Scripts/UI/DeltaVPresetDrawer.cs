using System;
using UnityEngine;
using UnityEngine.UI;

public class DeltaVPresetDrawer : MonoBehaviour
{
    [SerializeField] private Text mCaption;
    [SerializeField] private Text mDeltaV;
    [SerializeField] private Button mButton;

    public void Init(string caption, string deltaV, Action<string> onClick)
    {
        mCaption.text = caption;
        mDeltaV.text = deltaV;
        mButton.onClick.AddListener(() => onClick.Invoke(deltaV));
    }
}
