using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeVideoEffects.Node
{
    public interface INode
    {
        static Dictionary<int, INode>? Nodes { get; }
        public abstract Collection<Output> Outputs { get; set; }
        public abstract Collection<Input> Inputs { get; }

        public Output? GetOutputFromID(int id, int index) {
            INode? node;

            try
            {
                if ((node = Nodes[id]) == null)
                    throw new InvalidOperationException("NullReferenceException");
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException("KeyNotFoundException");
            }
            catch (ArgumentNullException)
            {
                throw new InvalidOperationException("ArgumentNullException");
            }
            
            if (node.Outputs[index] == null)
            {
                Collection<Input> inputs = node.Inputs;
                foreach (Input input in inputs)
                {
                    if (input.GetValue() == null)
                    {
                        Object? value = node.GetOutputFromID(input.GetConnect()[0], input.GetConnect()[1]);
                        if (value == null) throw new InvalidOperationException("NullReferenceException");
                        input.SetValue(value);
                    }
                }
                Outputs = Processor();
            }
            return node.Outputs[index];
        }

        public void AddInput(int port, int id, int index)
        {
            Inputs[port].SetConnect(id, index);
        }

        public void DeleteInput(int port, int id, int index)
        {
            Inputs[index].DeleteConnect();
        }

        public abstract Collection<Output> Processor();
    }
}
