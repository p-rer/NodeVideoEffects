namespace NodeVideoEffects.Type
{
    public interface PortValue
    {
        /// <summary>
        /// Type of value
        /// </summary>
        public System.Type Type { get => Value.GetType(); }
        /// <summary>
        /// Value of port
        /// </summary>
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

        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="value">Value</param>
        protected void _SetValue(object value);
    }
}
