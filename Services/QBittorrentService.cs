using ServerWatchAgent.Configuration;
using System.Text.Json;
using ServerWatchAgent.Models;
using System.Linq;

namespace ServerWatchAgent.Services;

public sealed class QBittorrentService
{
    private readonly QBittorrentConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public QBittorrentService(ServerWatchConfiguration configuration)
    {
        _configuration = configuration.QBittorrent;

        var handler = new HttpClientHandler
        {
            UseCookies = true
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(_configuration.BaseUrl)
        };
    }

    public async Task<string> GetVersionAsync()
    {
        var loginData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("username", _configuration.Username),
            new KeyValuePair<string, string>("password", _configuration.Password)
        ]);

        using var loginResponse =
            await _httpClient.PostAsync("/api/v2/auth/login", loginData);

        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadAsStringAsync();

        if (!string.Equals(loginResult.Trim(), "Ok.", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"qBittorrent authentication failed: {loginResult}");
        }

        return await _httpClient.GetStringAsync("/api/v2/app/version");
    }

    public async Task<string> GetTorrentsAsync()
    {
        var loginData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("username", _configuration.Username),
            new KeyValuePair<string, string>("password", _configuration.Password)
        ]);

        using var loginResponse =
            await _httpClient.PostAsync("/api/v2/auth/login", loginData);

        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadAsStringAsync();

        if (!string.Equals(loginResult.Trim(), "Ok.", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"qBittorrent authentication failed: {loginResult}");
        }

        return await _httpClient.GetStringAsync("/api/v2/torrents/info");
    }

    public async Task<List<QBittorrentTorrent>> GetTorrentModelsAsync()
    {
        var json = await GetTorrentsAsync();

        return JsonSerializer.Deserialize<List<QBittorrentTorrent>>(json)
            ?? [];
    }

    public async Task<List<DownloadStatus>> GetDownloadStatusesAsync()
    {
        var torrents = await GetTorrentModelsAsync();

        return torrents.Select(torrent => new DownloadStatus
        {
            Name = torrent.Name,
            SizeGb = Math.Round(torrent.SizeBytes / 1024.0 / 1024.0 / 1024.0, 2),
            ProgressPercent = (int)(torrent.Progress * 100.0),
            State = torrent.State,
            Suspicious = false
        }).ToList();
    }
}