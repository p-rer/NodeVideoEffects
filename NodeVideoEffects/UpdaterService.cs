using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Windows;
using Newtonsoft.Json.Linq;
using NodeVideoEffects.Utility;
using YukkuriMovieMaker.Plugin;

namespace NodeVideoEffects;

public partial class UpdaterService : IPlugin
{
    public UpdaterService()
    {
        Task.Run(async () =>
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("User-Agent", ".Net Application");

            try
            {
                var responseBody = await GetLatestRelease(client, "opencv", "opencv", false);
                var json = JObject.Parse(responseBody ??
                                         throw new InvalidOperationException("Failed to get the latest release."));
                var newVersion = ParseVersion(json["tag_name"]?.ToString());

                //var currentVersion = ParseVersion(ResourceLoader.FileLoad("git_tag.txt"));
                var currentVersion = new Version(0, 0, 1);
                if (newVersion <= currentVersion) return;
                Logger.Write(LogLevel.Info, $"A new version is available.\nCurrent version: {currentVersion}, Latest version: {newVersion}");
                var result = MessageBox.Show($"A new version is available. Do you want to update?\n{currentVersion} -> {newVersion}",
                    "Update", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var downloadUrl = json["assets"]?.First?["browser_download_url"]?.ToString();
                    if (downloadUrl != null)
                    {
                        var app = new ProcessStartInfo
                        {
                            FileName = "NodeVideoEffects.Updater.exe",
                            Arguments = downloadUrl
                        };

                        Process.Start(app);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.Write(LogLevel.Error, ex.Message, ex);
            }
        }).ConfigureAwait(false).GetAwaiter().GetResult();
    }
    public string Name => "NodeVideoEffects.Updater";

    private static async Task<string?> GetLatestRelease(HttpClient client, string owner, string repo, bool isPreRelease)
    {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");

        var releasesUrl = $"https://api.github.com/repos/{owner}/{repo}/releases";
        var response = await client.GetStringAsync(releasesUrl);
        var releases = JArray.Parse(response);

        return (from release in releases
            where release.Value<bool>("prerelease") == isPreRelease
            select release.ToString()).FirstOrDefault();
    }

    private static Version ParseVersion(string? input)
    {
        // Match the version format
        var match = VersionRegex().Match(input ?? throw new ArgumentNullException(nameof(input)));
        if (!match.Success) throw new FormatException("Invalid version format");
        // Parse the version components
        var major = int.Parse(match.Groups[1].Value);
        var minor = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
        var build = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : -1;
        var revision = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : -1;
        return revision == -1 ? new Version(major, minor, build) :
               build == -1 ? new Version(major, minor) : 
               new Version(major, minor, build, revision);
    }

    [GeneratedRegex(@"(?:[vV](?:er(?:sion)?)?|version)?\.?\s*(\d+)(?:\.(\d+))?(?:\.(\d+))?(?:\.(\d+))?")]
    private static partial Regex VersionRegex();
}