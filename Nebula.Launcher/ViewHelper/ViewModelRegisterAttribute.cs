using System;

namespace Nebula.Launcher.ViewHelper;

[AttributeUsage(AttributeTargets.Class)]
public class ViewModelRegisterAttribute : Attribute
{
    public ViewModelRegisterAttribute(Type? type = null, bool isSingleton = true)
    {
        Type = type;
        IsSingleton = isSingleton;
    }

    public Type? Type { get; }
    public bool IsSingleton { get; }
}