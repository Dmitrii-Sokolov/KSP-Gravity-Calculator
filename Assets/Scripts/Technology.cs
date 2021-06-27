using UnityEngine;

[CreateAssetMenu(fileName = "NewTechnology", menuName = "Technology")]
public class Technology : ScriptableObject
{
    public Sprite Sprite;

    public string Name;

    public int Level;

    public Part[] Parts;

    public static Technology[] GetAll()
    {
        return Resources.FindObjectsOfTypeAll<Technology>();
    }
}
