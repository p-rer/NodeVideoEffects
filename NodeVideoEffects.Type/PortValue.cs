namespace NodeVideoEffects.Type
{
    public interface PortValue
    {
        public System.Type Type { get => Value.GetType(); }
        public object Value { get; }
        public void SetValue(Object value)
        {
            try
            {
                Convert.ChangeType(value, Type);
                _SetValue(value);
            }
            catch (Exception _)
            {
                throw new TypeMismatchException(Value.GetType(), value.GetType());
            }
        }
        protected void _SetValue(object value);
    }
}
