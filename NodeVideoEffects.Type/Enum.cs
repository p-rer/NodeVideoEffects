using NodeVideoEffects.Control;
using System.Windows.Media;

namespace NodeVideoEffects.Type
{
    public class Enum : IPortValue
    {
        private int _value;
        public Enum(List<string> items, int value = 0)
        {
            Control = new EnumPort(items, value);
        }

        public System.Type Type => typeof(int);

        public object Value => _value;

        public Color Color => Colors.CornflowerBlue;

        public IControl Control { get; }

        public void Dispose() { }

        public void _SetValue(object? value)
        {
            _value = (int?)value ?? 0;
        }
    }
}
