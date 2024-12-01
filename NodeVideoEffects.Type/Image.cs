using NodeVideoEffects.Control;
using System.Windows.Media;

namespace NodeVideoEffects.Type
{
    public class Image : PortValue
    {
        ImageAndContext? _value;
        /// <summary>
        /// Create new Image object
        /// </summary>
        /// <param name="_value"></param>
        public Image(ImageAndContext? _value)
        {
            this._value = _value;
        }

        public System.Type Type { get => typeof(ImageAndContext); }
        public Color Color { get => Colors.Green; }

        /// <summary>
        /// Value contains image and device context
        /// </summary>
        public object Value { get { return _value; } }

        public void _SetValue(object? value)
        {
            _value = (ImageAndContext?)value;
        }

        public void Dispose()
        {
            _value?.Image?.Dispose();
            _value?.Context?.Dispose();
        }

        public IControl? Control => null;
    }
}
