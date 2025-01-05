using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Nebula.Shared;

public static class ServiceExt
{
    public static void AddServices(this IServiceCollection services)
    {
        foreach (var (type, inference) in GetServicesWithHelpAttribute(Assembly.GetExecutingAssembly()))
        {
            if (inference is null)
            {
                services.AddSingleton(type);
            }
            else
            {
                services.AddSingleton(inference, type);
            }
        }
    }
    
    private static IEnumerable<(Type,Type?)> GetServicesWithHelpAttribute(Assembly assembly) {
        foreach(Type type in assembly.GetTypes())
        {
            var attr = type.GetCustomAttribute<ServiceRegisterAttribute>();
            if (attr is not null) {
                yield return (type, attr.Inference);
            }
        }
    }
}

public sealed class ServiceRegisterAttribute : Attribute
{
    public Type? Inference { get; }
    public bool IsSingleton { get; }

    public ServiceRegisterAttribute(Type? inference = null, bool isSingleton = true)
    {
        IsSingleton = isSingleton;
        Inference = inference;
    }
}