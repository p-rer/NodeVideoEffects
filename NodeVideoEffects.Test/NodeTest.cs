using NodeVideoEffects.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeVideoEffects.Test
{
    [TestClass]
    public class AddTest
    {
        Add AddNode = new();
        [TestMethod]
        public async Task AddAsync()
        {
            await AddNode.Calculate();
            Assert.AreEqual((double)AddNode.Outputs[0].Value, 0);

            AddNode.Inputs[0].Value = 10.2;
            AddNode.Inputs[1].Value = -4.54;
            await AddNode.Calculate();
            Assert.AreEqual((double)AddNode.Outputs[0].Value, 5.66);
        }
    }
}
