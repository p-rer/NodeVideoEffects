using NodeVideoEffects.Control;
using System.Windows.Media;

namespace NodeVideoEffects.Type
{
    public interface PortValue
    {
        /// <summary>
        /// Type of value
        /// </summary>
        public System.Type Type { get; }
        /// <summary>
        /// Value of port
        /// </summary>
        public object Value { get; }
        public Color Color { get; }
        public void SetValue(object? value)
        {
            _SetValue(value);
        }

        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="value">Value</param>
        protected void _SetValue(object? value);

        /// <summary>
        /// Control for input port
        /// </summary>
        public IControl? Control { get; }
    }
}
