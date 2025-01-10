using System.ComponentModel;
using System.Windows.Media;

namespace NodeVideoEffects.Type
{
    /// <summary>
    /// Base class of nodes
    /// </summary>
    public abstract class INode : IDisposable
    {
        /// <summary>
        /// Get input ports
        /// </summary>
        public Input[] Inputs { get; }

        /// <summary>
        /// Get output ports
        /// </summary>
        public Output[] Outputs { get; }

        /// <summary>
        /// Name of this node
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Color of this node
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// Category of this node
        /// </summary>
        public string? Category { get; }

        public string Id { get; set; } = "";

        /// <summary>
        /// Create new node object
        /// </summary>
        /// <param name="inputs">Input ports</param>
        /// <param name="outputs">Output ports</param>
        /// <param name="name">Name of this node</param>
        /// <param name="category">Category of this node</param>
        protected INode(Input[] inputs, Output[] outputs, string name, string? category = null)
        {
            Inputs = inputs;
            Outputs = outputs;
            Name = name;
            Color = Colors.Transparent;
            Category = category;

            SubscribeToInputChanges();
        }

        /// <summary>
        /// Create new node object
        /// </summary>
        /// <param name="inputs">Input ports</param>
        /// <param name="outputs">Output ports</param>
        /// <param name="name">Name of this node</param>
        /// <param name="color">Color of this node</param>
        /// <param name="category">Category of this node</param>
        protected INode(Input[] inputs, Output[] outputs, string name, Color color, string? category = null)
        {
            Inputs = inputs;
            Outputs = outputs;
            Name = name;
            Color = color;
            Category = category;

            SubscribeToInputChanges();
        }

        ~INode()
        {
            if (Id != "")
                NodesManager.RemoveNode(Id);
            Dispose();
        }

        private void SubscribeToInputChanges()
        {
            foreach (var input in Inputs)
            {
                input.PropertyChanged += Input_PropertyChanged;
            }
        }

        protected void Input_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            foreach (var output in Outputs)
            {
                output.IsSuccess = false;
            }
        }

        public void SetInput(int index, object? value)
        {
            Inputs[index].Value = value;
        }

        /// <summary>
        /// Get value of output port
        /// </summary>
        /// <remarks>Override this function if the output value is a constant</remarks>
        /// <param name="index">Index of port</param>
        /// <returns>Value of output port</returns>
        public virtual object? GetOutput(int index)
        {
            return Outputs[index].Value;
        }

        public void SetInputConnection(int index, Connection connection)
        {
            Inputs[index].SetConnection(Id, index, connection.Id, connection.Index);
        }

        public void RemoveInputConnection(int index)
        {
            Inputs[index].RemoveConnection(Id, index);
        }

        /// <summary>
        /// Calculation on this node (async)
        /// </summary>
        /// <returns>The calculation task</returns>
        public abstract Task Calculate();

        public virtual void Dispose()
        {
            foreach (var input in Inputs)
                input.Dispose();
            foreach (var output in Outputs)
                output.Dispose();
            NodesManager.RemoveNode(Id);
        }
    }
}
