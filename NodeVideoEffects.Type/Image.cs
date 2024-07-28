using Vortice.Direct2D1;

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

        /// <summary>
        /// Value contains image and device context
        /// </summary>
        public object Value { get { return _value; } }

        public void _SetValue(object value)
        {
            _value = (ImageAndContext)value;
        }
    }
}
