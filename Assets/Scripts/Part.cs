using UnityEngine;

public class Part : ScriptableObject
{
    public string Name;
    public string NameRus;

    public float Cost;

    [Label("Mass, kg")]
    public float Mass;

    private static Part[] GetAllParts()
    {
        return Resources.FindObjectsOfTypeAll<Part>();
    }
}
