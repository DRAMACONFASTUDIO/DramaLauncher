using System;
using System.Collections.ObjectModel;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Popup;

namespace Nebula.Launcher.ViewModels;

[ViewModelRegister(typeof(ExceptionView), false)]
public class ExceptionViewModel : PopupViewModelBase
{
    public ExceptionViewModel() : base()
    {
        var e = new Exception("TEST");
        
        AppendError(e);
    }

    public ExceptionViewModel(IServiceProvider serviceProvider) : base(serviceProvider){}
    
    public override string Title => "Oopsie! Some shit is happened now!";

    public ObservableCollection<Exception> Errors { get; } = new();

    public void AppendError(Exception exception)
    {
        Errors.Add(exception);
        if(exception.InnerException != null) 
            AppendError(exception.InnerException);
    }
}
