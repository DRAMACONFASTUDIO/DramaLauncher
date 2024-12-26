using System;

namespace Nebula.Launcher.ViewHelper;

[AttributeUsage(AttributeTargets.Class)]
public class ViewRegisterAttribute : Attribute
{
    public Type Type { get; }
    public bool IsSingleton { get; }

    public ViewRegisterAttribute(Type type, bool isSingleton = true)
    {
        Type = type;
        IsSingleton = isSingleton;
    }
}