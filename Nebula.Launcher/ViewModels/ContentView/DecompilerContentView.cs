using System.IO;
using Nebula.Launcher.Services;
using Nebula.Launcher.ViewModels.Pages;
using Nebula.Shared.Utils;

namespace Nebula.Launcher.ViewModels.ContentView;

[ConstructGenerator]
public sealed partial class DecompilerContentView: ContentViewBase
{
    [GenerateProperty] private DecompilerService decompilerService {get;}

    public override void InitialiseWithData(ContentPath path, Stream stream, ContentEntry contentEntry)
    {
        base.InitialiseWithData(path, stream, contentEntry);
        decompilerService.OpenServerDecompiler(contentEntry.ServerName.ToRobustUrl());
    }

    private void unpackContent(Stream stream)
    {
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