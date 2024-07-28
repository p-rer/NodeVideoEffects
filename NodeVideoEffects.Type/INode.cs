﻿using System.ComponentModel;

namespace NodeVideoEffects.Type
{
    public abstract class INode
    {
        private Input[]? _inputs;
        private Output[]? _outputs;
        private string _name;

        private string _id;

        public Input[]? Inputs { get => _inputs; }
        public Output[]? Outputs { get => _outputs; }
        public string Name { get => _name; }
        public string Id { get => _id; }

        public INode(Input[]? inputs, Output[]? outputs, string name, string? id)
        {
            _inputs = inputs;
            _outputs = outputs;
            _name = name;

            _id = id??Guid.NewGuid().ToString("N");
            NodesManager.AddNode(_id, this);

            SubscribeToInputChanges();
        }

        ~INode()
        {
            NodesManager.RemoveNode(_id);
        }

        private void SubscribeToInputChanges()
        {
            if (_inputs == null)
                return;

            foreach (var input in _inputs)
            {
                input.PropertyChanged += Input_PropertyChanged;
            }
        }

        private void Input_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Input.Value))
            {
                foreach (Output output in _outputs)
                {
                    output.IsSuccess = false;
                }
            }
        }

        public void SetInput(int index, Object value)
        {
            _inputs[index].Value = value;
        }

        public virtual object? GetOutput(int index)
        {
            return _outputs[index].Value;
        }

        public void SetInputConnection(int index, Connection connection)
        {
            _inputs[index].SetConnection(_id, index, connection.id, connection.index);
        }

        public void RemoveInputConnection(int index)
        {
            _inputs[index].RemoveConnection(_id, index);
        }

        public abstract Task Calculate();
    }
}
