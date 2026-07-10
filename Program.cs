using System.Runtime.InteropServices;
using ServerWatchAgent.Models;
using ServerWatchAgent.Services;
using ServerWatchAgent.Configuration;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

var serverWatchConfiguration =
    builder.Configuration
        .GetSection("ServerWatch")
        .Get<ServerWatchConfiguration>()
    ?? throw new InvalidOperationException(
        "ServerWatch configuration is missing.");

builder.Services.AddSingleton(serverWatchConfiguration);

builder.Services.AddSingleton<StorageService>();
builder.Services.AddSingleton<ServiceHealthService>();
builder.Services.AddSingleton<ServerStatusService>();
builder.Services.AddSingleton<QBittorrentService>();

var app = builder.Build();

app.MapGet("/status", async (
    ServerStatusService serverStatusService,
    ServerWatchConfiguration configuration) =>
{
    var status = await serverStatusService.GetStatusAsync(
        configuration.StoragePath,
        configuration.StorageWarningThresholdGb,
        configuration.Services);

    return Results.Ok(status);
});

app.MapGet(
    "/qbittorrent/version",
    async (QBittorrentService qbittorrentService) =>
        await qbittorrentService.GetVersionAsync());

app.MapGet(
    "/qbittorrent/torrents",
    async (QBittorrentService qbittorrentService) =>
        Results.Content(
            await qbittorrentService.GetTorrentsAsync(),
            "application/json"));

app.MapGet(
    "/qbittorrent/models",
    async (QBittorrentService qbittorrentService) =>
        Results.Ok(
            await qbittorrentService.GetTorrentModelsAsync()));

app.MapGet(
    "/qbittorrent/downloads",
    async (QBittorrentService qbittorrentService) =>
        Results.Ok(
            await qbittorrentService.GetDownloadStatusesAsync()));

app.Run();
