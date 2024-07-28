using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using YukkuriMovieMaker.Player.Video;

namespace NodeVideoEffects.Type
{
    public static class NodesManager
    {
        private static Dictionary<string, INode>? _dictionary = new();
        private static int _frame;
        private static int _length;
        private static int _fps;

        public static async Task<Object> GetOutputValue(string id, int index)
        {
            INode node = _dictionary[id];
            if (node.Outputs[index].IsSuccess == true)
                return node.GetOutput(index);
            await node.Calculate();
            return node.GetOutput(index);
        }

        public static void NoticeOutputChanged(string id, int index)
        {
            _dictionary[id].Inputs[index].UpdatedConnectionValue();
        }
        public static void NoticeInputConnectionAdd(string iid, int iindex, string oid, int oindex)
        {
            _dictionary[oid].Outputs[oindex].AddConnection(iid, iindex);
        }
        public static void NoticeInputConnectionRemove(string iid, int iindex, string oid, int oindex)
        {
            _dictionary[oid].Outputs[oindex].RemoveConnection(iid, iindex);
        }
        public static void AddNode(string id, INode node) => _dictionary.Add(id, node);
        public static void RemoveNode(string id) => _dictionary.Remove(id);

        public static int _FRAME { get => _frame; }
        public static int _LENGTH { get => _length; }
        public static int _FPS { get => _fps; }

        internal static void SetInfo(EffectDescription info)
        {
            if (_frame != (_frame = info.ItemPosition.Frame))
            OnFrameChanged(nameof(_FRAME));
            if (_length != (_length = info.ItemDuration.Frame))
                OnFrameChanged(nameof(_LENGTH));
            if (_fps != (_fps = info.FPS))
                OnFrameChanged(nameof(_FPS));
        }

        public static event PropertyChangedEventHandler FrameChanged;
        public static event PropertyChangedEventHandler LengthChanged;
        public static event PropertyChangedEventHandler FPSChanged;

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
