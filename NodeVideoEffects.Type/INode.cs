using System.ComponentModel;
using System.Windows.Media;

namespace NodeVideoEffects.Type
{
    /// <summary>
    /// Base class of nodes
    /// </summary>
    public abstract class INode : IDisposable
    {
        private Input[]? _inputs;
        private Output[]? _outputs;
        private string _name;

        private string _id;
        private readonly Color _color;

        /// <summary>
        /// Get input ports
        /// </summary>
        public Input[]? Inputs { get => _inputs; }

        /// <summary>
        /// Get output ports
        /// </summary>
        public Output[]? Outputs { get => _outputs; }

        /// <summary>
        /// Name of this node
        /// </summary>
        public string Name { get => _name; }

        /// <summary>
        /// Color of this node
        /// </summary>
        public Color Color => _color;

        public string Id { get => _id; set => _id = value; }
        /// <summary>
        /// Create new node object
        /// </summary>
        /// <param name="inputs">Input ports</param>
        /// <param name="outputs">Output ports</param>
        /// <param name="name">Name of this node</param>
        /// <param name="id">ID of this node(Set this null)</param>
        public INode(Input[]? inputs, Output[]? outputs, string name)
        {
            _inputs = inputs;
            _outputs = outputs;
            _name = name;
            _color = Colors.Transparent;

            SubscribeToInputChanges();
        }

        public INode(Input[]? inputs, Output[]? outputs, string name, Color color)
        {
            _inputs = inputs;
            _outputs = outputs;
            _name = name;
            _color = color;

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

        /// <summary>
        /// Get value of output port
        /// </summary>
        /// <remarks>Override this function if the output value is a constant</remarks>
        /// <param name="index">Index of port</param>
        /// <returns>Value of output port</returns>
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

        /// <summary>
        /// Calculation on this node (async)
        /// </summary>
        /// <returns>The calculation task</returns>
        public abstract Task Calculate();

        public virtual void Dispose() { }
    }
}
