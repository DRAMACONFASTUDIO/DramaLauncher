using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Nebula.Shared;

public static class ServiceExt
{
    public static void AddServices(this IServiceCollection services)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) AddServices(services, assembly);
    }

    public static void AddServices(this IServiceCollection services, Assembly assembly)
    {
        foreach (var (type, inference) in GetServicesWithHelpAttribute(assembly))
        {
            Console.WriteLine("[ServiceMng] ADD SERVICE " + type);
            if (inference is null)
                services.AddSingleton(type);
            else
                services.AddSingleton(inference, type);
        }
    }

    private static IEnumerable<(Type, Type?)> GetServicesWithHelpAttribute(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            var attr = type.GetCustomAttribute<ServiceRegisterAttribute>();
            if (attr is not null) yield return (type, attr.Inference);
        }
    }
}

public sealed class ServiceRegisterAttribute : Attribute
{
    public ServiceRegisterAttribute(Type? inference = null, bool isSingleton = true)
    {
        IsSingleton = isSingleton;
        Inference = inference;
    }

    public Type? Inference { get; }
    public bool IsSingleton { get; }
}