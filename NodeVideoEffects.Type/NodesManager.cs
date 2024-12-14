using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace NodeVideoEffects.Type
{
    public static class NodesManager
    {
        private static Dictionary<string, INode> _dictionary = new();
        private static List<string> _items = new();
        private static Dictionary<string, ID2D1Image?> _images = new();

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
                INode node = _dictionary[id];
                if (node.GetType().FullName == "NodeVideoEffects.Nodes.Basic.InputNode")
                    return new ImageAndContext(_images[id[0..id.IndexOf('-')]]);
                if (node.Outputs[index].IsSuccess)
                    return node.GetOutput(index);
                await node.Calculate();
                return node.GetOutput(index);
            }
            catch
            {
                return null;
            }
        }

        public static INode? GetNode(string id)
        {
            INode? node;
            if (_dictionary.TryGetValue(id, out node))
                return node;
            return null;
        }

        public static PropertyChangedEventHandler InputUpdated = delegate { };

        public static void SetInput(string id, ID2D1Image image)
        {
            _images[id] = image;
            InputUpdated.Invoke(null, new(null));
        }

        public static bool CheckConnection(string iid, string oid)
        {
            bool result = true;
            if (!(_dictionary[iid].Outputs == null || _dictionary[iid].Outputs?.Length == 0))
            {
                foreach (Output output in _dictionary[iid].Outputs)
                {
                    if (!result)
                        break;
                    if (output.Connection.Count == 0)
                        break;
                    foreach (Connection connection in output.Connection)
                    {
                        if (connection.id == oid)
                        {
                            result = false;
                            break;
                        }
                        if (result)
                            result = CheckConnection(connection.id, oid);
                    }
                }
            }
            return result;
        }

        public static void NoticeOutputChanged(string id, int index)
        {
            _dictionary[id].Inputs[index].UpdatedConnectionValue();
        }
        public static void NoticeInputConnectionAdd(string iid, int iindex, string oid, int oindex)
        {
            try
            {
                _dictionary[oid].Outputs[oindex].AddConnection(iid, iindex);
            }
            catch { }
        }
        public static void NoticeInputConnectionRemove(string iid, int iindex, string oid, int oindex)
        {
            try
            {
                _dictionary[oid].Outputs[oindex].RemoveConnection(iid, iindex);
            }
            catch { }
        }
        public static void AddNode(string id, INode node) => _dictionary.Add(id, node);
        public static void RemoveNode(string id) => _dictionary.Remove(id);
        public static bool AddItem(string id)
        {
            if (_items.Contains(id)) return false;
            _items.Add(id);
            _images.Add(id, null);
            return true;
        }
        public static void RemoveItem(string id)
        {
            if (_images.TryGetValue(id, out ID2D1Image? image))
            {
                image?.Dispose();
            }
            _images.Remove(id);
            _items.Remove(id);
            _dictionary.Where(kvp => kvp.Key.StartsWith(id))
                       .Select(kvp => _dictionary.Remove(kvp.Key));
        }

        private static Dictionary<string, IGraphicsDevicesAndContext> contexts = [];
        public static void SetContext(string id, IGraphicsDevicesAndContext context)
        {
            if (contexts.ContainsKey(id))
                contexts[id] = context;
            else
                contexts.Add(id, context);
        }
        public static IGraphicsDevicesAndContext GetContext(string id)
        {
            return contexts[id];
        }

        /// <summary>
        /// Now frame
        /// </summary>
        public static Dictionary<string, int> _FRAME { get; private set; } = [];
        /// <summary>
        /// Length of the item
        /// </summary>
        public static Dictionary<string, int> _LENGTH { get; private set; } = [];
        /// <summary>
        /// FPS of the YMM4 project
        /// </summary>
        public static Dictionary<string, int> _FPS { get; private set; } = [];

        private static Dictionary<string, EffectDescription> _EffectDescription = [];

        internal static void SetInfo(string id, EffectDescription info)
        {
            if (!_FRAME.ContainsKey(id))
            {
                _FRAME.Add(id, info.ItemPosition.Frame);
                _LENGTH.Add(id, info.ItemDuration.Frame);
                _FPS.Add(id, info.FPS);
                _EffectDescription.Add(id, info);
            }
            else
            {
                if (info.ItemPosition.Frame != _FRAME[id])
                {
                    _FRAME[id] = info.ItemPosition.Frame;
                    OnFrameChanged(nameof(_FRAME));
                }
                if (_LENGTH[id] != info.ItemDuration.Frame)
                {
                    _LENGTH[id] = info.ItemDuration.Frame;
                    OnLengthChanged(nameof(_LENGTH));
                }
                if (_FPS[id] != info.FPS)
                {
                    _FPS[id] = info.FPS;
                    OnFPSChanged(nameof(_FPS));
                }
                _EffectDescription[id] = info;
            }
        }

        public static EffectDescription GetInfo(string id) => _EffectDescription[id];

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
        public static event PropertyChangedEventHandler FPSChanged = delegate { };

        private static void OnFrameChanged(string propertyName)
        {
            FrameChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        private static void OnLengthChanged(string propertyName)
        {
            LengthChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        private static void OnFPSChanged(string propertyName)
        {
            FPSChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}
