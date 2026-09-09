using System.IO;
using Newtonsoft.Json;

namespace Node.Updater;

internal sealed class PluginUpdateState
{
    internal const string FileName = ".update-state.json";

    public DateTime NextCheckNotBeforeUtc { get; set; } = DateTime.MinValue;

    public string? PendingApplyVersion { get; set; }

    public string? PendingStagingDirectory { get; set; }

    public static PluginUpdateState Load(string pluginDirectory)
    {
        try
        {
            var path = Path.Combine(pluginDirectory, FileName);
            if (!File.Exists(path)) return new PluginUpdateState();

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<PluginUpdateState>(json) ?? new PluginUpdateState();
        }
        catch
        {
            return new PluginUpdateState();
        }
    }

    public void Save(string pluginDirectory)
    {
        try
        {
            var path = Path.Combine(pluginDirectory, FileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        catch
        {
            // ignore
        }
    }
}