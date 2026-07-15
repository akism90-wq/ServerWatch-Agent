using System.Globalization;
using System.Text.RegularExpressions;

namespace ServerWatchAgent.Services;

public static class ReleaseNameParser
{
    public static (string Title, string Subtitle) Parse(
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

    private static (string Title, string Subtitle)
        ParseMovieName(string cleanedName)
    {
        var metadataFreeName =
            RemoveReleaseMetadata(cleanedName);

        var yearMatch = Regex.Match(
            metadataFreeName,
            @"\b(?:19|20)\d{2}\b");

        if (yearMatch.Success)
        {
            var title = FormatTitle(
                metadataFreeName[..yearMatch.Index]);

            return (
                title,
                yearMatch.Value);
        }

        return (
            FormatTitle(metadataFreeName),
            string.Empty);
    }

    private static (string Title, string Subtitle)
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
            var title = FormatTitle(
                cleanedName[..episodeMatch.Index]);

            var episodeCode =
                NormaliseEpisodeCode(episodeMatch.Value);

            var remainingText = cleanedName[
                (episodeMatch.Index + episodeMatch.Length)..]
                .Trim();

            var episodeTitle = FormatTitle(
                RemoveReleaseMetadata(remainingText));

            var subtitle =
                string.IsNullOrWhiteSpace(episodeTitle)
                    ? episodeCode
                    : $"{episodeCode} - {episodeTitle}";

            return (title, subtitle);
        }

        var seasonMatch = Regex.Match(
            cleanedName,
            @"\bS(?<season>\d{1,2})\b",
            RegexOptions.IgnoreCase);

        if (seasonMatch.Success)
        {
            var title = FormatTitle(
                cleanedName[..seasonMatch.Index]);

            var seasonNumber = int.Parse(
                seasonMatch.Groups["season"].Value,
                CultureInfo.InvariantCulture);

            return (
                title,
                $"Season {seasonNumber}");
        }

        return (
            FormatTitle(RemoveReleaseMetadata(cleanedName)),
            string.Empty);
    }

    private static string NormaliseEpisodeCode(string value)
    {
        var xFormatMatch = Regex.Match(
            value,
            @"^(?<season>\d{1,2})x(?<episode>\d{1,3})$",
            RegexOptions.IgnoreCase);

        if (!xFormatMatch.Success)
        {
            return value.ToUpperInvariant();
        }

        var season = int.Parse(
            xFormatMatch.Groups["season"].Value,
            CultureInfo.InvariantCulture);

        var episode = int.Parse(
            xFormatMatch.Groups["episode"].Value,
            CultureInfo.InvariantCulture);

        return $"S{season:00}E{episode:00}";
    }

    private static string RemoveReleaseMetadata(string value)
    {
        var metadataMatch = Regex.Match(
            value,
            @"\b(?:720p|1080p|2160p|WEB|WEB-DL|WEBRip|" +
            @"BluRay|HDTV|DCPRIP|REMUX|x264|x265|H264|" +
            @"H265|HEVC|HDR|DV|AAC|DDP|DTS)\b",
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