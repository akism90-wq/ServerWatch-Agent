using ServerWatchAgent.Configuration;
using System.Text.Json;
using ServerWatchAgent.Models;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ServerWatchAgent.Services;

public sealed class QBittorrentService
{
    private readonly QBittorrentConfiguration _configuration;
    private readonly HttpClient _httpClient;

    private readonly DownloadWarningConfiguration _downloadWarnings;

    public QBittorrentService(ServerWatchConfiguration configuration)
    {
        _configuration = configuration.QBittorrent;
        _downloadWarnings = configuration.DownloadWarnings;

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
        var torrents = (await GetTorrentModelsAsync())
            .Where(torrent =>
                torrent.State is
                    "downloading" or
                    "forcedDL" or
                    "stalledDL" or
                    "queuedDL" or
                    "metaDL" or
                    "checkingDL" or
                    "allocating")
            .ToList();

        return torrents.Select(torrent =>
        {
            var sizeGb = Math.Round(
                torrent.SizeBytes / 1024.0 / 1024.0 / 1024.0,
                2);

            var suspicious =
                torrent.Category.Contains(
                    "sonarr",
                    StringComparison.OrdinalIgnoreCase)
                    ? sizeGb > _downloadWarnings.TvEpisodeThresholdGb
                    : torrent.Category.Contains(
                        "radarr",
                        StringComparison.OrdinalIgnoreCase)
                        ? sizeGb > _downloadWarnings.MovieThresholdGb
                        : false;

            var amountLeftGb = Math.Round(
                torrent.AmountLeftBytes / 1024.0 / 1024.0 / 1024.0,
                2);
                
            var (title, subtitle) =
                ReleaseNameParser.Parse(
                    torrent.Name,
                    torrent.Category);

            return new DownloadStatus
            {
                Name = torrent.Name,
                Title = title,
                Subtitle = subtitle,
                SizeGb = sizeGb,
                ProgressPercent = (int)(torrent.Progress * 100.0),
                DownloadSpeedBytesPerSecond =
                    torrent.DownloadSpeedBytesPerSecond,
                EtaSeconds = torrent.EtaSeconds,
                AmountLeftGb = amountLeftGb,
                State = GetFriendlyState(torrent.State),
                Suspicious = suspicious
            };
        }).ToList();
    }
    
    private static string GetFriendlyState(string state)
    {
        return state switch
        {
            "downloading" => "Downloading",
            "forcedDL" => "Downloading",
            "stalledDL" => "Stalled",
            "queuedDL" => "Queued",
            "metaDL" => "Fetching metadata",
            "checkingDL" => "Checking",
            "allocating" => "Allocating",
            _ => state
        };
    }

    private static (string DisplayName, string DisplayDetail)
        GetDisplayNameAndDetail(
            string rawName,
            string category)
    {
        var cleanedName = Regex.Replace(
            rawName,
            @"[._]+",
            " ");

        cleanedName = Regex.Replace(
            cleanedName,
            @"\s+",
            " ").Trim();

        if (category.Contains(
                "sonarr",
                StringComparison.OrdinalIgnoreCase))
        {
            return ParseTvName(cleanedName);
        }

        if (category.Contains(
                "radarr",
                StringComparison.OrdinalIgnoreCase))
        {
            return ParseMovieName(cleanedName);
        }

        return (
            FormatTitle(RemoveReleaseMetadata(cleanedName)),
            string.Empty);
    }

    private static (string DisplayName, string DisplayDetail)
        ParseTvName(string cleanedName)
    {
        var episodeMatch = Regex.Match(
            cleanedName,
            @"\bS\d{1,2}E\d{1,3}(?:E\d{1,3})*\b",
            RegexOptions.IgnoreCase);

        if (!episodeMatch.Success)
        {
            episodeMatch = Regex.Match(
                cleanedName,
                @"\b\d{1,2}x\d{1,3}\b",
                RegexOptions.IgnoreCase);
        }

        if (episodeMatch.Success)
        {
            var displayName = FormatTitle(
                cleanedName[..episodeMatch.Index]);

            var episodeCode = episodeMatch.Value.ToUpperInvariant();

            var detailStart =
                episodeMatch.Index + episodeMatch.Length;

            var remainingText =
                cleanedName[detailStart..].Trim();

            var episodeTitle = FormatTitle(
                RemoveReleaseMetadata(remainingText));

            var displayDetail = string.IsNullOrWhiteSpace(episodeTitle)
                ? episodeCode
                : $"{episodeCode} · {episodeTitle}";

            return (displayName, displayDetail);
        }

        var seasonMatch = Regex.Match(
            cleanedName,
            @"\bS(?<season>\d{1,2})\b",
            RegexOptions.IgnoreCase);

        if (seasonMatch.Success)
        {
            var displayName = FormatTitle(
                cleanedName[..seasonMatch.Index]);

            var seasonNumber = int.Parse(
                seasonMatch.Groups["season"].Value,
                CultureInfo.InvariantCulture);

            return (
                displayName,
                $"Season {seasonNumber}");
        }

        return (
            FormatTitle(RemoveReleaseMetadata(cleanedName)),
            string.Empty);
    }

    private static (string DisplayName, string DisplayDetail)
        ParseMovieName(string cleanedName)
    {
        var metadataFreeName =
            RemoveReleaseMetadata(cleanedName);

        var yearMatch = Regex.Match(
            metadataFreeName,
            @"\b(?:19|20)\d{2}\b");

        if (yearMatch.Success)
        {
            var displayName = FormatTitle(
                metadataFreeName[..yearMatch.Index]);

            return (
                displayName,
                yearMatch.Value);
        }

        return (
            FormatTitle(metadataFreeName),
            string.Empty);
    }

    private static string RemoveReleaseMetadata(string value)
    {
        var metadataMatch = Regex.Match(
            value,
            @"\b(?:720p|1080p|2160p|WEB|WEB-DL|WEBRip|BluRay|" +
            @"HDTV|DCPRIP|REMUX|x264|x265|H264|H265|HEVC|" +
            @"HDR|DV|AAC|DDP|DTS)\b",
            RegexOptions.IgnoreCase);

        return metadataMatch.Success
            ? value[..metadataMatch.Index].Trim()
            : value.Trim();
    }

    private static string FormatTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
            value.Trim().ToLowerInvariant());
    }
}