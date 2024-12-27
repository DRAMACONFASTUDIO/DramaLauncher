using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Nebula.Launcher.FileApis;
using Robust.LoaderApi;

namespace Nebula.Launcher.Services;

[ServiceRegister]
public class AssemblyService
{
    private readonly List<Assembly> _assemblies = new();
    private readonly DebugService _debugService;

    public AssemblyService(DebugService debugService)
    {
        _debugService = debugService;
    }

    public IReadOnlyList<Assembly> Assemblies => _assemblies;

    public AssemblyApi Mount(IFileApi fileApi)
    {
        var asmApi = new AssemblyApi(fileApi);
        AssemblyLoadContext.Default.Resolving += (context, name) => OnAssemblyResolving(context, name, asmApi);
        AssemblyLoadContext.Default.ResolvingUnmanagedDll += LoadContextOnResolvingUnmanaged;

        return asmApi;
    }

    public bool TryGetLoader(Assembly clientAssembly, [NotNullWhen(true)] out ILoaderEntryPoint? loader)
    {
        loader = null;
        // Find ILoaderEntryPoint with the LoaderEntryPointAttribute
        var attrib = clientAssembly.GetCustomAttribute<LoaderEntryPointAttribute>();
        if (attrib == null)
        {
            Console.WriteLine("No LoaderEntryPointAttribute found on Robust.Client assembly!");
            return false;
        }

        var type = attrib.LoaderEntryPointType;
        if (!type.IsAssignableTo(typeof(ILoaderEntryPoint)))
        {
            Console.WriteLine("Loader type '{0}' does not implement ILoaderEntryPoint!", type);
            return false;
        }

        loader = (ILoaderEntryPoint)Activator.CreateInstance(type)!;
        return true;
    }

    public bool TryOpenAssembly(string name, AssemblyApi assemblyApi, [NotNullWhen(true)] out Assembly? assembly)
    {
        if (!TryOpenAssemblyStream(name, assemblyApi, out var asm, out var pdb))
        {
            assembly = null;
            return false;
        }

        assembly = AssemblyLoadContext.Default.LoadFromStream(asm, pdb);
        _debugService.Log("LOADED ASSEMBLY " + name);


        if (!_assemblies.Contains(assembly)) _assemblies.Add(assembly);

        asm.Dispose();
        pdb?.Dispose();
        return true;
    }

    public bool TryOpenAssemblyStream(string name, AssemblyApi assemblyApi, [NotNullWhen(true)] out Stream? asm,
        out Stream? pdb)
    {
        asm = null;
        pdb = null;

        if (!assemblyApi.TryOpen($"{name}.dll", out asm))
            return false;

        assemblyApi.TryOpen($"{name}.pdb", out pdb);
        return true;
    }

    private Assembly? OnAssemblyResolving(AssemblyLoadContext context, AssemblyName name, AssemblyApi assemblyApi)
    {
        _debugService.Debug("Resolving assembly from FileAPI: " + name.Name);
        return TryOpenAssembly(name.Name!, assemblyApi, out var assembly) ? assembly : null;
    }

    private IntPtr LoadContextOnResolvingUnmanaged(Assembly assembly, string unmanaged)
    {
        var ourDir = Path.GetDirectoryName(typeof(AssemblyApi).Assembly.Location);
        var a = Path.Combine(ourDir!, unmanaged);

        _debugService.Debug($"Loading dll lib: {a}");

        if (NativeLibrary.TryLoad(a, out var handle))
            return handle;

        return IntPtr.Zero;
    }
}