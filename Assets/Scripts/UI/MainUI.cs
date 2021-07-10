using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [Serializable]
    private class CanvasButtonPair
    {
        public GameObject Canvas;
        public Button Button;
    }

    [SerializeField] private List<CanvasButtonPair> mCanvasButtonPairs;
    [SerializeField] private ScriptableObject[] mScriptableObjects;

    //TODO Make fuel tanks table

    void Start()
    {
        SetCanvas(mCanvasButtonPairs.First());
        foreach (var pair in mCanvasButtonPairs)
            pair.Button.onClick.AddListener(() => SetCanvas(pair));
    }

    private void SetCanvas(CanvasButtonPair pair)
    {
        foreach (var item in mCanvasButtonPairs)
        {
            item.Button.interactable = pair.Canvas != item.Canvas;
            item.Canvas.SetActive(pair.Canvas == item.Canvas);
        }
    }
}
