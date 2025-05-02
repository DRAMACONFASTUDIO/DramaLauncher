using System;

namespace Nebula.UpdateResolver.Configuration;

public static class ConVarBuilder
{
    public static ConVar<T> Build<T>(string name, T? defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("ConVar name cannot be null or whitespace.", nameof(name));

        return new ConVar<T>(name, defaultValue);
    }
}