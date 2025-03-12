using System;
using System.Globalization;
using System.IO;
using Avalonia.Platform;
using Fluent.Net;
using Nebula.Shared;
using Nebula.Shared.Services;

namespace Nebula.Launcher.Services;

[ConstructGenerator, ServiceRegister]
public partial class LocalisationService
{
    [GenerateProperty] private ConfigurationService ConfigurationService { get; }

    private CultureInfo _currentCultureInfo = CultureInfo.CurrentCulture;
    private MessageContext _currentMessageContext;
    
    private void Initialise()
    {
       // LoadLanguage(CultureInfo.GetCultureInfo(ConfigurationService.GetConfigValue(LauncherConVar.CurrentLang)!));
    }

    public void LoadLanguage(CultureInfo cultureInfo)
    {
        _currentCultureInfo = cultureInfo;
        using var fs = AssetLoader.Open(new Uri($@"Assets/lang/{_currentCultureInfo.EnglishName}.ftl"));
        using var sr = new StreamReader(fs);
        
        var options = new MessageContextOptions { UseIsolating = false };
        var mc = new MessageContext(cultureInfo.EnglishName, options);
        var errors = mc.AddMessages(sr);
        foreach (var error in errors)
        {
            Console.WriteLine(error);
        }

        _currentMessageContext = mc;
    }

    private void InitialiseInDesignMode()
    {
        Initialise();
    }
}

