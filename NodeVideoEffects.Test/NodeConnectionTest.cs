using NodeVideoEffects.Nodes.Math;
using NodeVideoEffects.Type;

namespace NodeVideoEffects.Test
{
    [TestClass]
    public class NodeConnectionTest
    {
        [TestMethod]
        public async Task ConnectionTestAsync()
        {
            double result;
            AddNode addNode = new();
            PowNode powNode = new();

            addNode.SetInput(0, -13.0);
            addNode.SetInput(1, 15.0);

            powNode.SetInput(0, 8.0);
            powNode.SetInputConnection(1, new(addNode.Id, 0));

            result = (double)await NodesManager.GetOutputValue(powNode.Id, 0);
            Assert.AreEqual(result, 64.0);

            return;
        }
    }
}
