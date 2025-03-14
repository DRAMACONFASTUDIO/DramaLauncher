using System;
using System.IO;
using Nebula.Launcher.ViewModels.Pages;

namespace Nebula.Launcher.ViewModels.ContentView;
public abstract class ContentViewBase : ViewModelBase, IDisposable
{
    public virtual void InitialiseWithData(ContentPath path, Stream stream, ContentEntry contentEntry)
    {
    }
    public virtual void Dispose()
    {
    }
}