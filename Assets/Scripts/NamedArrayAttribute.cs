using UnityEngine;

public class NamedArrayAttribute : PropertyAttribute
{
    public readonly string[] names;

    public NamedArrayAttribute(params string[] names)
    {
        this.names = names;
    }
}