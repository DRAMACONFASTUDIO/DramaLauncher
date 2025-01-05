using System;

namespace Nebula.Launcher.ViewHelper;

[AttributeUsage(AttributeTargets.Class)]
public class ViewModelRegisterAttribute : Attribute
{
    public Type? Type { get; }
    public bool IsSingleton { get; }

    public ViewModelRegisterAttribute(Type? type = null, bool isSingleton = true)
    {
        Type = type;
        IsSingleton = isSingleton;
    }
}