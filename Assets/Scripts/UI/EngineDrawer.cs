using UnityEngine;
using UnityEngine.UI;

public class EngineDrawer : MonoBehaviour
{
    [SerializeField] private Text mEngineCount;
    [SerializeField] private Image mEngineImage;
    [SerializeField] private Text mEngineRate;
    [SerializeField] private Text mEngineAlias;

    public void Init(int engineCount, Engine engine, float rate = 1f)
    {
        mEngineCount.text = engineCount.ToString();
        mEngineImage.sprite = engine.Sprite;
        mEngineImage.color = engine.Sprite == null ? Color.clear : Color.white;
        mEngineAlias.text = engine.Alias;
        mEngineRate.text = rate > 0.99f ? string.Empty : (100f * rate).ToString("N", Locale.Format);
    }
}
