using System.Linq;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Test()
    {
        var techs = Technology.GetAll();
        var parts = Part.GetAll();

        foreach (var part in parts)
        {
            var tech = techs.FirstOrDefault(t => t.Parts.Contains(part));
            if (tech == null)
                Debug.LogWarning($"No technology for part: {part.Alias}");
        }
    }
}
