using NodeVideoEffects.Control;
using System.Windows.Media;

namespace NodeVideoEffects.Type
{
    public class Image : IPortValue
    {
        private ImageWrapper _value;
        private IControl? _control;

        /// <summary>
        /// Create new Image object
        /// </summary>
        /// <param name="value"></param>
        public Image(ImageWrapper? value)
        {
            _value = value ?? new ImageWrapper();
        }

        public System.Type Type => typeof(ImageWrapper);
        public Color Color => Colors.Green;

        /// <summary>
        /// Value contains image and device context
        /// </summary>
        public object Value => _value;

        public void _SetValue(object? value)
        {
            _value = (ImageWrapper?)value ?? new ImageWrapper();
        }

        public void Dispose()
        {
            _value.Image?.Dispose();
        }

        public IControl Control => _control ??= new NoControlPort();
    }
}
