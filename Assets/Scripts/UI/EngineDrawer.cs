using UnityEngine;
using UnityEngine.UI;

public class EngineDrawer : MonoBehaviour
{
    [SerializeField] private Text mEngineCount;
    [SerializeField] private Image mEngineImage;
    [SerializeField] private Text mEngineRate;
    [SerializeField] private Text mEngineAlias;

    public void Init(int engineCount, Engine engine, float rate = 0f)
    {
        mEngineCount.text = engineCount.ToString();
        mEngineImage.sprite = engine.Sprite;
        mEngineAlias.text = engine.Alias;
        mEngineRate.text = (100f * rate).ToString("N", Locale.Format);
        mEngineRate.gameObject.SetActive(rate > 0f);
    }
}
