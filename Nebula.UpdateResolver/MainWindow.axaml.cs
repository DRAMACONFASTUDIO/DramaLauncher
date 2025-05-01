using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Nebula.Shared.FileApis.Interfaces;
using Nebula.Shared.Models;
using Nebula.Shared.Services;

namespace Nebula.UpdateResolver;

public partial class MainWindow : Window
{
    private readonly ConfigurationService _configurationService;
    private readonly RestService _restService;
    private readonly HttpClient _httpClient = new HttpClient();
    public IReadWriteFileApi FileApi { get; set; }
    
    public MainWindow(FileService fileService, ConfigurationService configurationService, RestService restService)
    {
        _configurationService = configurationService;
        _restService = restService;
        InitializeComponent();
        FileApi = fileService.CreateFileApi("app");
        
    }

    private async Task DownloadFiles()
    {
        var info = await EnsureFiles();

        foreach (var file in info.ToDelete)
        {
            FileApi.Remove(file.Path);
        }

        foreach (var file in info.ToDownload)
        {
            using var response = _httpClient.GetAsync(
                _configurationService.GetConfigValue(UpdateConVars.UpdateCacheUrl) 
                + "/" + file.Hash
                );
            response.Result.EnsureSuccessStatusCode();
            await using var stream = await response.Result.Content.ReadAsStreamAsync();
            FileApi.Save(file.Path, stream);
        }
    }

    private async Task<ManifestEnsureInfo> EnsureFiles()
    {
        var manifest = await _restService.GetAsync<LauncherManifest>(
            _configurationService.GetConfigValue(UpdateConVars.UpdateCacheUrl)!, CancellationToken.None);

        var toDownload = new HashSet<LauncherManifestEntry>();
        var toDelete = new HashSet<LauncherManifestEntry>();
        
        if (_configurationService.TryGetConfigValue(UpdateConVars.CurrentLauncherManifest, out var currentManifest))
        {
            foreach (var file in currentManifest.Entries)
            {
                if(!manifest.Entries.Contains(file))
                    toDelete.Add(file);
            }

            foreach (var file in manifest.Entries)
            {
                if(!currentManifest.Entries.Contains(file))
                    toDownload.Add(file);
            }
        }
        else
        {
            _configurationService.SetConfigValue(UpdateConVars.CurrentLauncherManifest, manifest);
            toDownload = manifest.Entries;
        }

        return new ManifestEnsureInfo(toDownload, toDelete);
    }
}

public record struct ManifestEnsureInfo(HashSet<LauncherManifestEntry> ToDownload, HashSet<LauncherManifestEntry> ToDelete);