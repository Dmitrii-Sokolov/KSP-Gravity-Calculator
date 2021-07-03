using UnityEngine;

public class Part : ScriptableObject
{
    public Sprite Sprite;

    public string Alias;
    public string Name;

    public float Cost;

    [Label("Mass, kg")]
    public float Mass;

    public static Part[] GetAll()
    {
        return Resources.FindObjectsOfTypeAll<Part>();
    }

    public static T[] GetAll<T>() where T : Part
    {
        return Resources.FindObjectsOfTypeAll<T>();
    }
}
