using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace Node.Updater;

internal static class PluginUpdateChecker
{
    private const string RepositoryOwner = "p-rer";
    private const string RepositoryName = "Node";

    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);

    private static readonly HttpClient HttpClient = CreateHttpClient();

    private static bool _initialized;

#pragma warning disable CA2255
    [ModuleInitializer]
    internal static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        _ = RunStartupCheckAsync();
    }
#pragma warning restore CA2255

    private static async Task RunStartupCheckAsync()
    {
        try
        {
            var pluginDirectory = GetPluginDirectory();
            var state = PluginUpdateState.Load(pluginDirectory);
            if (state.PendingApplyVersion != null &&
                TryParseVersion(state.PendingApplyVersion, out var pendingVersion) &&
                pendingVersion <= GetCurrentVersion())
            {
                CleanupPendingState(pluginDirectory, state);
                state = new PluginUpdateState();
            }

            if (state.PendingApplyVersion != null)
                return;

            if (DateTime.UtcNow < state.NextCheckNotBeforeUtc)
                return;

            var app = await WaitForApplicationAsync().ConfigureAwait(false);
            if (app == null) return;

            var result = await CheckForUpdateAsync(GetCurrentVersion(), CancellationToken.None)
                .ConfigureAwait(false);

            state.NextCheckNotBeforeUtc = DateTime.UtcNow + CheckInterval;
            state.Save(pluginDirectory);

            if (result is not { HasUpdate: true })
                return;

            await app.Dispatcher.BeginInvoke(() => ShowNotification(result, pluginDirectory));
        }
        catch
        {
            // ignore
        }
    }

    private static void ShowNotification(UpdateCheckResult result, string pluginDirectory)
    {
        try
        {
            var window = new UpdateNotificationWindow(result, pluginDirectory);
            window.Show();
        }
        catch
        {
            // ignore
        }
    }

    private static async Task<Application?> WaitForApplicationAsync()
    {
        for (var i = 0; i < 60; i++)
        {
            var app = Application.Current;
            if (app != null) return app;
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        }

        return null;
    }

    internal static Version GetCurrentVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
    }

    internal static string GetPluginDirectory()
    {
        var location = Assembly.GetExecutingAssembly().Location;
        return Path.GetDirectoryName(location) is { Length: > 0 } dir ? dir : AppContext.BaseDirectory;
    }

    internal static async Task<UpdateCheckResult?> CheckForUpdateAsync(
        Version currentVersion,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/releases/latest";
            using var response = await HttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var release = JObject.Parse(json);

            var tagName = release.Value<string>("tag_name");
            if (string.IsNullOrWhiteSpace(tagName)) return null;
            if (!TryParseVersion(tagName, out var latestVersion)) return null;

            var assetUrl = release["assets"]?
                .FirstOrDefault(a =>
                    (a.Value<string>("name") ?? "").EndsWith(".ymme", StringComparison.OrdinalIgnoreCase))?
                .Value<string>("browser_download_url");

            var releasePageUrl = release.Value<string>("html_url");
            var releaseNotes = release.Value<string>("body");

            return new UpdateCheckResult(
                latestVersion > currentVersion,
                latestVersion,
                releasePageUrl,
                assetUrl,
                releaseNotes);
        }
        catch
        {
            return null;
        }
    }

    internal static async Task<string> DownloadAndStageUpdateAsync(
        UpdateCheckResult update,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(update.DownloadUrl))
            throw new InvalidOperationException("ダウンロード対象が見つかりませんでした。");

        var pluginDirectory = GetPluginDirectory();
        var stagingDir = Path.Combine(pluginDirectory, ".update-staging", update.LatestVersion.ToString());

        if (Directory.Exists(stagingDir))
            Directory.Delete(stagingDir, true);
        Directory.CreateDirectory(stagingDir);

        var zipPath = Path.Combine(Path.GetTempPath(), $"node-update-{Guid.NewGuid():N}.zip");
        try
        {
            using (var response = await HttpClient
                       .GetAsync(update.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                       .ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                await using var httpStream =
                    await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                await using var fileStream =
                    new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await httpStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();

            await ZipFile.ExtractToDirectoryAsync(zipPath, stagingDir, true, cancellationToken);
        }
        finally
        {
            try
            {
                if (File.Exists(zipPath)) File.Delete(zipPath);
            }
            catch
            {
                // 一時ファイルの削除失敗は無視する。
            }
        }

        return stagingDir;
    }

    internal static void ScheduleApplyOnNextExit(UpdateCheckResult update, string stagingDir)
    {
        var pluginDirectory = GetPluginDirectory();

        var state = PluginUpdateState.Load(pluginDirectory);
        state.PendingApplyVersion = update.LatestVersion.ToString();
        state.PendingStagingDirectory = stagingDir;
        state.Save(pluginDirectory);

        var currentProcess = Process.GetCurrentProcess();

        var scriptPath = Path.Combine(Path.GetTempPath(), $"node-update-apply-{Guid.NewGuid():N}.ps1");
        var stateFilePath = Path.Combine(pluginDirectory, PluginUpdateState.FileName);
        var script = BuildApplyScript(
            currentProcess.Id, stagingDir, pluginDirectory, stateFilePath, scriptPath);
        File.WriteAllText(scriptPath, script);

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"{scriptPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        Process.Start(psi);
    }

    private static string BuildApplyScript(
        int waitForProcessId,
        string stagingDir,
        string pluginDirectory,
        string stateFilePath,
        string selfPath)
    {
        return $"""
                $ErrorActionPreference = 'SilentlyContinue'
                Wait-Process -Id {waitForProcessId} -ErrorAction SilentlyContinue
                Start-Sleep -Seconds 2
                Copy-Item -Path "{stagingDir}\*" -Destination "{Directory.GetParent(pluginDirectory)?.FullName}" -Recurse -Force
                Remove-Item -Path "{stagingDir}" -Recurse -Force -ErrorAction SilentlyContinue
                Remove-Item -Path "{stateFilePath}" -Force -ErrorAction SilentlyContinue
                Remove-Item -Path "{selfPath}" -Force -ErrorAction SilentlyContinue
                """;
    }

    private static void CleanupPendingState(string pluginDirectory, PluginUpdateState state)
    {
        try
        {
            if (state.PendingStagingDirectory != null && Directory.Exists(state.PendingStagingDirectory))
                Directory.Delete(state.PendingStagingDirectory, true);
        }
        catch
        {
            // ignore
        }

        try
        {
            var path = Path.Combine(pluginDirectory, PluginUpdateState.FileName);
            if (File.Exists(path)) File.Delete(path);
        }
        catch
        {
            // ignore
        }
    }

    internal static void PostponeCheck(string pluginDirectory)
    {
        var state = PluginUpdateState.Load(pluginDirectory);
        state.NextCheckNotBeforeUtc = DateTime.UtcNow + CheckInterval;
        state.Save(pluginDirectory);
    }

    internal static bool TryParseVersion(string tagName, out Version version)
    {
        var trimmed = tagName.TrimStart('v', 'V');

        var span = trimmed.AsSpan();
        var end = 0;
        while (end < span.Length && (char.IsAsciiDigit(span[end]) || span[end] == '.')) end++;

        return Version.TryParse(span[..end].ToString(), out version!);
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Node-Plugin-Updater");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return client;
    }
}

internal sealed record UpdateCheckResult(
    bool HasUpdate,
    Version LatestVersion,
    string? ReleasePageUrl,
    string? DownloadUrl,
    string? ReleaseNotes);