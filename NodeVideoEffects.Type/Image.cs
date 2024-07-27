using Vortice.Direct2D1;

namespace NodeVideoEffects.Type
{
    public class Image : PortValue
    {
        ID2D1Image? _value;
        public Image(ID2D1Bitmap? _value)
        {
            this._value = _value;
        }

        public object Value { get { return _value; } }

        public void _SetValue(object value)
        {
            _value = (ID2D1Image)value;
        }
    }
}
