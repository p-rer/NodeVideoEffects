using System.ComponentModel;
using System.Reflection;
using NodeVideoEffects.Utility;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace NodeVideoEffects.Core;

public static class NodesManager
{
    private static readonly Dictionary<string, NodeLogic> Dictionary = [];
    private static readonly List<string> Items = [];

    private static readonly Dictionary<string, IGraphicsDevicesAndContext> Contexts = [];

    private static readonly Dictionary<string, EffectDescription> EffectDescription = [];

    /// <summary>
    ///     Now frame
    /// </summary>
    public static Dictionary<string, int> Frame { get; } = [];

    /// <summary>
    ///     Length of the item
    /// </summary>
    public static Dictionary<string, int> Length { get; } = [];

    /// <summary>
    ///     FPS of the YMM4 project
    /// </summary>
    public static Dictionary<string, int> Fps { get; } = [];

    /// <summary>
    /// Get output port value from node id and port index
    /// </summary>
    /// <param name="id">Node id</param>
    /// <param name="index">Port index</param>
    /// <returns>Task of getting value (result is the value)</returns>
    public static async Task<object?> GetOutputValue(string id, int index)
    {
        try
        {
            var node = Dictionary[id];
            switch (node.GetType().FullName)
            {
                case "NodeVideoEffects.Nodes.Basic.Frame":
                    return (float)Frame[id[..id.IndexOf('-')]];
            }

            try
            {
                if (GetInfo(id[..id.IndexOf('-')]).Usage == TimelineSourceUsage.Playing &&
                    node.Outputs[index].IsSuccess)
                    return node.GetOutput(index);
            }
            catch
            {
                /*ignored*/
            }

            await TaskTracker.RunTrackedTask(node.Calculate);

            var result = node.GetOutput(index);
            Logger.Write(LogLevel.Debug,
                $"Get output value from node {id} ({node.Name}), index {index} - {result?.ToString() ?? "null"}");
            return result;
        }
        catch (Exception e)
        {
            Logger.Write(LogLevel.Error, e.Message, e);
            return null;
        }
    }

    public static NodeLogic? GetNode(string id)
    {
        return Dictionary.GetValueOrDefault(id);
    }

    public static bool CheckConnection(string inId, string outId)
    {
        var result = true;
        if (Dictionary[inId].Outputs.Length == 0) return result;
        foreach (var output in Dictionary[inId].Outputs)
        {
            if (!result)
                break;
            if (output.Connection.Count == 0)
                break;
            foreach (var connection in output.Connection)
            {
                if (connection.Id == outId)
                {
                    result = false;
                    break;
                }

                if (result)
                    result = CheckConnection(connection.Id, outId);
            }
        }

        return result;
    }

    public static void NotifyOutputChanged(string id, int index, bool isControlChanged = false)
    {
        Task.Run(() => Dictionary[id].Inputs[index].UpdatedConnectionValue(isControlChanged));
    }

    public static void NotifyInputConnectionAdd(string inId, int inIndex, string outId, int outIndex)
    {
        Task.Run(() =>
        {
            try
            {
                Dictionary[outId].Outputs[outIndex].AddConnection(inId, inIndex);
            }
            catch (Exception e)
            {
                Logger.Write(LogLevel.Error, e.Message, e);
            }
        });
    }

    public static void NotifyInputConnectionRemove(string inId, int inIndex, string outId, int outIndex)
    {
        Task.Run(() =>
        {
            try
            {
                Dictionary[outId].Outputs[outIndex].RemoveConnection(inId, inIndex);
            }
            catch (Exception e)
            {
                Logger.Write(LogLevel.Error, e.Message, e);
            }
        });
    }

    public static void AddNode(string id, NodeLogic node)
    {
        if (!Dictionary.TryAdd(id, node)) Dictionary[id] = node;
    }

    public static void RemoveNode(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        Dictionary.TryGetValue(id, out var node);
        if (node == null) return;
        node.Outputs.Select(output => output.Connection).ToList()
            .ForEach(connections => connections
                .ForEach(connection =>
                {
                    if (connection.Id == "") return;
                    Dictionary[connection.Id].Inputs[connection.Index].RemoveConnection("", 0);
                }));
        node.Inputs.Select((input, i) =>
                (input.PortInfo, i)).ToList()
            .ForEach(port =>
            {
                if (port.PortInfo.Id == "") return;
                Dictionary[port.PortInfo.Id].Outputs[port.PortInfo.Index].RemoveConnection(node.Id, port.i);
            });
        node.Dispose();
        Dictionary.Remove(id);
    }

    public static bool AddItem(string id)
    {
        if (string.IsNullOrEmpty(id) || Items.Contains(id)) return false;
        Items.Add(id);
        return true;
    }

    public static void RemoveItem(string id)
    {
        Dictionary.Select(kvp => kvp.Key)
            .ToList()
            .ForEach(key =>
            {
                if (!key.StartsWith(id)) return;
                Dictionary.TryGetValue(key, out var node);
                node?.Dispose();
                Dictionary.Remove(key);
            });
        Items.Remove(id);
    }

    public static void SetContext(string id, IGraphicsDevicesAndContext context)
    {
        if (!Contexts.TryAdd(id, context))
            Contexts[id] = context;
        Dictionary.Where(kvp => kvp.Key.StartsWith(id)).ToList()
            .ForEach(kvp => kvp.Value.UpdateContext(context.DeviceContext));
    }

    public static IGraphicsDevicesAndContext GetContext(string id)
    {
        return Contexts[id];
    }

    public static void SetInfo(string id, EffectDescription info)
    {
        if (Assembly.GetCallingAssembly().GetName().Name != "NodeVideoEffects")
            return;
        if (!Frame.ContainsKey(id))
        {
            Frame.Add(id, info.ItemPosition.Frame);
            Length.Add(id, info.ItemDuration.Frame);
            Fps.Add(id, info.FPS);
            EffectDescription.Add(id, info);
        }
        else
        {
            if (info.ItemPosition.Frame != Frame[id])
            {
                Frame[id] = info.ItemPosition.Frame;
                OnFrameChanged(nameof(Frame));
            }

            if (Length[id] != info.ItemDuration.Frame)
            {
                Length[id] = info.ItemDuration.Frame;
                OnLengthChanged(nameof(Length));
            }

            if (Fps[id] != info.FPS)
            {
                Fps[id] = info.FPS;
                OnFPSChanged(nameof(Fps));
            }

            EffectDescription[id] = info;
        }
    }

    public static EffectDescription GetInfo(string id)
    {
        return EffectDescription[id];
    }

    /// <summary>
    /// Now frame has changed
    /// </summary>
    public static event PropertyChangedEventHandler? FrameChanged;

    /// <summary>
    /// Length of the item has changed
    /// </summary>
    public static event PropertyChangedEventHandler? LengthChanged;

    /// <summary>
    /// FPS of the YMM4 project has changed
    /// </summary>
    public static event PropertyChangedEventHandler? FpsChanged;

    private static void OnFrameChanged(string propertyName)
    {
        FrameChanged?.Invoke(false, new PropertyChangedEventArgs(propertyName));
    }

    private static void OnLengthChanged(string propertyName)
    {
        LengthChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }

    private static void OnFPSChanged(string propertyName)
    {
        FpsChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }
}