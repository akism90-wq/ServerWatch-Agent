using System.Runtime.InteropServices;
using ServerWatchAgent.Models;

namespace ServerWatchAgent.Services;

public sealed class ServerStatusService
{
    private readonly StorageService _storageService;
    private readonly ServiceHealthService _serviceHealthService;
    private readonly QBittorrentService _qbittorrentService;

    public ServerStatusService(
        StorageService storageService,
        ServiceHealthService serviceHealthService,
        QBittorrentService qbittorrentService)
    {
        _storageService = storageService;
        _serviceHealthService = serviceHealthService;
        _qbittorrentService = qbittorrentService;
    }

    public async Task<ServerStatus> GetStatusAsync(
        string storagePath,
        double storageWarningThresholdGb,
        IReadOnlyCollection<ServiceConfiguration> configuredServices)
    {
        var serviceChecks = configuredServices.Select(service =>
            _serviceHealthService.GetStatusAsync(
                service.Name,
                service.Url));

        var serviceStatuses = await Task.WhenAll(serviceChecks);

        var downloads =
            await _qbittorrentService.GetDownloadStatusesAsync();

        return new ServerStatus
        {
            Server = new ServerInfo
            {
                Name = Environment.MachineName,
                Online = true,
                OperatingSystem = RuntimeInformation.OSDescription
            },

            Storage = _storageService.GetStatus(
                storagePath,
                storageWarningThresholdGb),

            Services = [.. serviceStatuses]
        };
    }
}