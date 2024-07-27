﻿using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;

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
            if (node.Outputs[index].Value != null)
                return node.Outputs[index].Value;
            foreach (Input input in node.Inputs)
            {
                if (input.Value == null)
                    input.Value = Convert.ChangeType(
                        await GetOutputValue(input.Connection.Value.id, input.Connection.Value.index),
                        input.Type);
            }
            await node.Calculate();
            return node.Outputs[index].Value;
        }

        public static bool IsSuccessOutput(string id, int index) => _dictionary[id].Outputs[index].IsSuccess;

        public static void AddNode(string id, INode node) => _dictionary.Add(id, node);
        public static void RemoveNode(string id) => _dictionary.Remove(id);

        public static int _FRAME { get => _frame; }
        public static int _LENGTH { get => _length; }
        public static int _FPS { get => _fps; }

        internal static void SetFrame(int frame)
        {
            _frame = frame;
            OnFrameChanged(nameof(_FRAME));
        }

        internal static void SetLength(int length)
        {
            _length = length;
        }

        internal static void SetFPS(int fps)
        {
            _fps = fps;
        }

        public static event PropertyChangedEventHandler FrameChanged;

        private static void OnFrameChanged(string propertyName)
        {
            FrameChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}
