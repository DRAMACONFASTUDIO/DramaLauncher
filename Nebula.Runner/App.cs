using Nebula.Shared;
using Nebula.Shared.Services;

namespace Nebula.Runner;

[ServiceRegister]
public class App(DebugService debugService)
{

    public void Run(string[] args)
    {
        debugService.Log("HELLO!!! " + string.Join(" ",args));
    }
}