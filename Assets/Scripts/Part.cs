using UnityEngine;

public class Part : ScriptableObject
{
    public Sprite Sprite;

    public string Alias;
    public string Name;

    public float Cost;

    [Label("Mass, kg")]
    public float Mass;

    private static Part[] GetAllParts()
    {
        return Resources.FindObjectsOfTypeAll<Part>();
    }
}
