using System.IO;
using Nebula.Launcher.Services;
using Nebula.Launcher.ViewModels.Pages;

namespace Nebula.Launcher.ViewModels.ContentView;

[ConstructGenerator]
public sealed partial class DecompilerContentView: ContentViewBase
{
    [GenerateProperty] private DecompilerService decompilerService {get;}

    public override void InitialiseWithData(ContentPath path, Stream stream)
    {
        base.InitialiseWithData(path, stream);
        var myTempFile = Path.Combine(Path.GetTempPath(), "tempie.dll");

        var sw = new FileStream(myTempFile, FileMode.Create, FileAccess.Write, FileShare.None);
        stream.CopyTo(sw);
        sw.Dispose();
        stream.Dispose();

        decompilerService.OpenDecompiler(myTempFile);
    }

    protected override void Initialise()
    {
    }

    protected override void InitialiseInDesignMode()
    {
    }
}