namespace NodeVideoEffects.Type
{
    public abstract class INode
    {
        private Input[]? _inputs;
        private Output[]? _outputs;
        private string _name;

        public Input[]? Inputs { get => _inputs; }
        public Output[]? Outputs { get => _outputs; }
        public string Name { get => _name; }

        public INode(Input[]? inputs, Output[]? outputs, string name)
        {
            _inputs = inputs;
            _outputs = outputs;
            _name = name;
        }

        public void SetInput(int index, Object value)
        {
            _inputs[index].Value = value;
        }

        public abstract Task Calculate();
    }
}
