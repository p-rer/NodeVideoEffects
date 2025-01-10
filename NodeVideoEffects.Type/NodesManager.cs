﻿using NodeVideoEffects.Utility;
using System.ComponentModel;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace NodeVideoEffects.Type
{
    public static class NodesManager
    {
        private static readonly Dictionary<string, INode> Dictionary = [];
        private static readonly List<string> Items = [];
        private static readonly Dictionary<string, ID2D1Image?> Images = [];

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
                if (node.GetType().FullName == "NodeVideoEffects.Nodes.Basic.InputNode")
                    return new ImageAndContext(Images[id[..id.IndexOf('-')]]);
                if (node.Outputs[index].IsSuccess)
                    return node.GetOutput(index);
                await node.Calculate();
                return node.GetOutput(index);
            }
            catch(Exception e)
            {
                Logger.Write(LogLevel.Error, e.Message);
                return null;
            }
        }

        public static INode? GetNode(string id) => Dictionary.GetValueOrDefault(id);

        public static PropertyChangedEventHandler InputUpdated = delegate { };

        public static void SetInput(string id, ID2D1Image? image)
        {
            Images[id] = image;
            InputUpdated.Invoke(null, new PropertyChangedEventArgs(null));
        }

        public static bool CheckConnection(string iid, string oid)
        {
            var result = true;
            if (Dictionary[iid].Outputs.Length == 0) return result;
            foreach (var output in Dictionary[iid].Outputs)
            {
                if (!result)
                    break;
                if (output.Connection.Count == 0)
                    break;
                foreach (var connection in output.Connection)
                {
                    if (connection.Id == oid)
                    {
                        result = false;
                        break;
                    }
                    if (result)
                        result = CheckConnection(connection.Id, oid);
                }
            }
            return result;
        }

        public static void NoticeOutputChanged(string id, int index)
        {
            Dictionary[id].Inputs[index].UpdatedConnectionValue();
        }
        public static void NoticeInputConnectionAdd(string iid, int iindex, string oid, int oindex)
        {
            try
            {
                Dictionary[oid].Outputs[oindex].AddConnection(iid, iindex);
            }
            catch (Exception e)
            {
                Logger.Write(LogLevel.Error, e.Message);
            }
        }
        public static void NoticeInputConnectionRemove(string inId, int inIndex, string outId, int outIndex)
        {
            try
            {
                Dictionary[outId].Outputs[outIndex].RemoveConnection(inId, inIndex);
            }
            catch (Exception e)
            {
                Logger.Write(LogLevel.Error, e.Message);
            }
        }
        public static void AddNode(string id, INode node) => Dictionary.Add(id, node);
        public static void RemoveNode(string id) => Dictionary.Remove(id);
        public static bool AddItem(string id)
        {
            if (Items.Contains(id)) return false;
            Items.Add(id);
            Images.Add(id, null);
            return true;
        }
        public static void RemoveItem(string id)
        {
            if (Images.TryGetValue(id, out ID2D1Image? image))
            {
                image?.Dispose();
            }
            Images.Remove(id);
            Items.Remove(id);
            _ = Dictionary.Where(kvp => kvp.Key.StartsWith(id))
                .Select(kvp => Dictionary.Remove(kvp.Key));
        }

        private static readonly Dictionary<string, IGraphicsDevicesAndContext> Contexts = [];
        public static void SetContext(string id, IGraphicsDevicesAndContext context)
        {
            if (!Contexts.TryAdd(id, context))
                Contexts[id] = context;
        }
        public static IGraphicsDevicesAndContext GetContext(string id)
        {
            return Contexts[id];
        }

        /// <summary>
        /// Now frame
        /// </summary>
        public static Dictionary<string, int> Frame { get; private set; } = [];
        /// <summary>
        /// Length of the item
        /// </summary>
        public static Dictionary<string, int> Length { get; private set; } = [];
        /// <summary>
        /// FPS of the YMM4 project
        /// </summary>
        public static Dictionary<string, int> Fps { get; private set; } = [];

        private static readonly Dictionary<string, EffectDescription> EffectDescription = [];

        internal static void SetInfo(string id, EffectDescription info)
        {
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

        public static EffectDescription GetInfo(string id) => EffectDescription[id];

        /// <summary>
        /// Now frame has changed
        /// </summary>
        public static event PropertyChangedEventHandler FrameChanged = delegate { };
        /// <summary>
        /// Length of the item has changed
        /// </summary>
        public static event PropertyChangedEventHandler LengthChanged = delegate { };
        /// <summary>
        /// FPS of the YMM4 project has changed
        /// </summary>
        public static event PropertyChangedEventHandler FpsChanged = delegate { };

        private static void OnFrameChanged(string propertyName)
        {
            FrameChanged(null, new PropertyChangedEventArgs(propertyName));
        }

        private static void OnLengthChanged(string propertyName)
        {
            LengthChanged(null, new PropertyChangedEventArgs(propertyName));
        }

        private static void OnFPSChanged(string propertyName)
        {
            FpsChanged(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}
