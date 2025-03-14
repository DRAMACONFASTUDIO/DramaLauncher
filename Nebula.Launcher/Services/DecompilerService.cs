
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Nebula.Launcher.ViewModels.Popup;
using Nebula.Shared;
using Nebula.Shared.FileApis;
using Nebula.Shared.FileApis.Interfaces;
using Nebula.Shared.Services;

namespace Nebula.Launcher.Services;

[ConstructGenerator, ServiceRegister]
public sealed partial class DecompilerService
{
    [GenerateProperty] private ConfigurationService ConfigurationService { get; } 
    [GenerateProperty] private PopupMessageService PopupMessageService {get;}
    [GenerateProperty] private ViewHelperService ViewHelperService {get;}

    private HttpClient _httpClient = new HttpClient();

    private static string fullPath = Path.Join(FileService.RootPath,"ILSpy");
    private static string executePath = Path.Join(fullPath, "ILSpy.exe");

    public async void OpenDecompiler(string path){
        await EnsureILSpy();
        var startInfo = new ProcessStartInfo(){
            FileName = executePath,
            Arguments = path
        };
        Process.Start(startInfo);
    }

    private void Initialise(){}
    private void InitialiseInDesignMode(){}

    private async Task EnsureILSpy(){
        if(!Directory.Exists(fullPath))
            await Download();
    }

    private async Task Download(){
        using var loading = ViewHelperService.GetViewModel<LoadingContextViewModel>();
        loading.LoadingName = "Download ILSpy";
        loading.SetJobsCount(1);
        PopupMessageService.Popup(loading);
        using var response = await _httpClient.GetAsync(ConfigurationService.GetConfigValue(LauncherConVar.ILSpyUrl));
        using var zipArchive = new ZipArchive(await response.Content.ReadAsStreamAsync());
        Directory.CreateDirectory(fullPath);
        zipArchive.ExtractToDirectory(fullPath);
    }
}