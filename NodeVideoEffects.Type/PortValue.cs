namespace NodeVideoEffects.Type
{
    public interface PortValue
    {
        public System.Type Type { get; }
        public object Value { get; }
        private void BeforeSetValue(Object value)
        {
            if (value.GetType() == Value.GetType())
            {
                SetValue(value);
            }
            else
            {
                throw new TypeMismatchException(Value.GetType(), value.GetType());
            }
        }
        public void SetValue(object value);
    }
}
