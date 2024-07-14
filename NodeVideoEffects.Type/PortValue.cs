namespace NodeVideoEffects.Type
{
    public interface PortValue
    {
        public System.Type Type { get; }
        public object Value { get; }
        public void SetValue(Object value)
        {
            if (value.GetType() == Value.GetType())
            {
                BeforeSetValue(value);
            }
            else
            {
                throw new TypeMismatchException(Value.GetType(), value.GetType());
            }
        }
        private void BeforeSetValue(object value) { }
    }
}
