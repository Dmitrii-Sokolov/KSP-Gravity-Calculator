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
        public Canvas Canvas;
        public Button Button;
    }

    [SerializeField] private List<CanvasButtonPair> mCanvasButtonPairs;
    [SerializeField] private ScriptableObject[] mScriptableObjects;

    //TODO Medium Фильтр по технологиям + сохранение в реестр
    //TODO Minor Сделать таблицу баков с топливом, проверить актуальность
    //TODO Minor Попробовать перевести мод с порядком на русский

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
            item.Canvas.enabled = pair.Canvas == item.Canvas;
        }
    }
}
