using Vortice.Direct2D1;

namespace NodeVideoEffects.Type
{
    public class Image : PortValue
    {
        ImageAndContext? _value;
        public Image(ImageAndContext? _value)
        {
            this._value = _value;
        }

        public object Value { get { return _value; } }

        public void _SetValue(object value)
        {
            _value = (ImageAndContext)value;
        }
    }
}
