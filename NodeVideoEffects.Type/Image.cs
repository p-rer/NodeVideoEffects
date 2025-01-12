using NodeVideoEffects.Control;
using System.Windows.Media;

namespace NodeVideoEffects.Type
{
    public class Image : IPortValue
    {
        private ImageAndContext _value;
        private IControl? _control;

        /// <summary>
        /// Create new Image object
        /// </summary>
        /// <param name="value"></param>
        public Image(ImageAndContext? value)
        {
            _value = value ?? new ImageAndContext();
        }

        public System.Type Type => typeof(ImageAndContext);
        public Color Color => Colors.Green;

        /// <summary>
        /// Value contains image and device context
        /// </summary>
        public object Value => _value;

        public void _SetValue(object? value)
        {
            _value = (ImageAndContext?)value ?? new ImageAndContext();
        }

        public void Dispose()
        {
            _value.Image?.Dispose();
        }

        public IControl Control => _control ??= new NoControlPort();
    }
}
