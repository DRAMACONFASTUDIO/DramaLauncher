using System.Net;
using FluentFTP;
using Nebula.Shared.FileApis.Interfaces;

namespace Nebula.Shared.FileApis;

public class FtpFileApi : IWriteFileApi, IDisposable
{
    private readonly string _ftpHost;
    private readonly string _username;
    private readonly string _password;

    private readonly FtpClient Client;

    public FtpFileApi(string ftpHost, string username, string password)
    {
        _ftpHost = ftpHost;
        _username = username;
        _password = password;
        Client = CreateClient();
        Client.AutoConnect();
    }

    public bool Save(string path, Stream input)
    {
        try
        {
            var result = Client.UploadStream(input, path, FtpRemoteExists.Overwrite, true);
            return result == FtpStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    public bool Remove(string path)
    {
        try
        {
            Client.DeleteFile(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Has(string path)
    {
        try
        {
            return Client.FileExists(path);
        }
        catch
        {
            return false;
        }
    }

    private FtpClient CreateClient()
    {
        var client = new FtpClient(_ftpHost, _username, _password);
        client.Config.EncryptionMode = FtpEncryptionMode.None;
        client.Config.ValidateAnyCertificate = true; 
        return client;
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}