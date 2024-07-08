using System.Collections.ObjectModel;

namespace NodeVideoEffects.Node
{
    public class Input
    {
        private int[] connect_to;
        private Object value;
        
        public Input() { }

        public void SetConnect(int id, int index) => connect_to = [id, index];

        public void DeleteConnect() => connect_to = [-1, -1];

        public int[] GetConnect() => connect_to;

        public void SetValue(Object value) => this.value = value;

        public Object GetValue() => value;
    }
}