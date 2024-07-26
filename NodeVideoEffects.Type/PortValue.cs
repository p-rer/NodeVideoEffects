namespace NodeVideoEffects.Type
{
    public interface PortValue
    {
        public System.Type Type { get => Value.GetType(); }
        public object Value { get; }
        public void SetValue(Object value)
        {
            if (value.GetType() == Value.GetType())
            {
                _SetValue(value);
            }
            else
            {
                throw new TypeMismatchException(Value.GetType(), value.GetType());
            }
        }
        protected void _SetValue(object value);
    }
}
