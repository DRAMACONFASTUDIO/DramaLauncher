using System;

namespace Nebula.UpdateResolver.Configuration;

public class ConVar<T>
{
    public ConVar(string name, T? defaultValue = default)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DefaultValue = defaultValue;
    }

    public string Name { get; }
    public Type Type => typeof(T);
    public T? DefaultValue { get; }
}