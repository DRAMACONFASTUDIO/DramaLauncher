using System;

namespace Nebula.Launcher.ViewHelper;

public class ViewRegisterAttribute : Attribute
{
    public Type Type { get; }

    public ViewRegisterAttribute(Type type)
    {
        Type = type;
    }
}