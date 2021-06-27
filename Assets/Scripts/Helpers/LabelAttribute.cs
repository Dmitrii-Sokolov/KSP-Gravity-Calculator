using UnityEngine;

public class LabelAttribute : PropertyAttribute
{
    public string NewName { get; private set; }
    public LabelAttribute(string name)
    {
        NewName = name;
    }
}