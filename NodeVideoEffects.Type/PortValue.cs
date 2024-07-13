namespace NodeVideoEffects.Type
{
    public interface PortValue
    {
        public System.Type Type { get; }
        public object Value { get; }
        public void SetValue(object value);
    }
}
