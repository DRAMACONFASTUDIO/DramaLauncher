using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Nebula.Shared.FileApis;
using Nebula.Shared.FileApis.Interfaces;
using Nebula.Shared.Models;
using Nebula.Shared.Services;
using Tmds.DBus.Protocol;

namespace Nebula.UpdateResolver;

public partial class MainWindow : Window
{
    private readonly ConfigurationService _configurationService;
    private readonly RestService _restService;
    private readonly HttpClient _httpClient = new HttpClient();
    public FileApi FileApi { get; set; }
    
    public MainWindow(FileService fileService, ConfigurationService configurationService, RestService restService)
    {
        _configurationService = configurationService;
        _restService = restService;
        InitializeComponent();
        FileApi = (FileApi)fileService.CreateFileApi("app");

        Start();
    }

    private async Task Start()
    {
        var info = await EnsureFiles();
        Log("Downloading files...");

        foreach (var file in info.ToDelete)
        {
            Log("Deleting " + file.Path);
            FileApi.Remove(file.Path);
        }

        var loadedManifest = info.FilesExist;
        Save(loadedManifest);

        var count = info.ToDownload.Count;
        var resolved = 0;

        foreach (var file in info.ToDownload)
        {
            using var response = await _httpClient.GetAsync(
                _configurationService.GetConfigValue(UpdateConVars.UpdateCacheUrl) 
                + "/" + file.Hash);
            
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync();
            FileApi.Save(file.Path, stream);
            resolved++;
            Log("Saving " + file.Path, (int)(resolved/(float)count*100f));
            
            loadedManifest.Add(file);
            Save(loadedManifest);
        }
        Log("Download finished. Running launcher...");
        
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet.exe",
            Arguments = Path.Join(FileApi.RootPath,"Nebula.Launcher.dll"),
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8
        });
        
        Thread.Sleep(2000);
        
        Environment.Exit(0);
    }

    private async Task<ManifestEnsureInfo> EnsureFiles()
    {
        Log("Ensuring launcher manifest...");
        var manifest = await _restService.GetAsync<LauncherManifest>(
            new Uri(_configurationService.GetConfigValue(UpdateConVars.UpdateCacheUrl)! + "/manifest.json"), CancellationToken.None);
        
        var toDownload = new HashSet<LauncherManifestEntry>();
        var toDelete = new HashSet<LauncherManifestEntry>();
        var filesExist = new HashSet<LauncherManifestEntry>();
        
        Log("Manifest loaded!");
        if (_configurationService.TryGetConfigValue(UpdateConVars.CurrentLauncherManifest, out var currentManifest))
        {
            Log("Delta manifest loaded!");
            foreach (var file in currentManifest.Entries)
            {
                if (!manifest.Entries.Contains(file))
                    toDelete.Add(file);
                else
                    filesExist.Add(file);
            }

            foreach (var file in manifest.Entries)
            {
                if(!currentManifest.Entries.Contains(file))
                    toDownload.Add(file);
            }
        }
        else
        {
            toDownload = manifest.Entries;
        }
        
        Log("Saving launcher manifest...");

        return new ManifestEnsureInfo(toDownload, toDelete, filesExist);
    }

    private void Log(string message, int percentage = 0)
    {
        ProgressLabel.Content = message;
        if (percentage == 0)
            PercentLabel.Content = "";
        else
            PercentLabel.Content = percentage + "%";
        
        Console.WriteLine(message);
    }

    private void Save(HashSet<LauncherManifestEntry> entries)
    {
        _configurationService.SetConfigValue(UpdateConVars.CurrentLauncherManifest, new LauncherManifest(entries));
    }
}

public record struct ManifestEnsureInfo(HashSet<LauncherManifestEntry> ToDownload, HashSet<LauncherManifestEntry> ToDelete, HashSet<LauncherManifestEntry> FilesExist);